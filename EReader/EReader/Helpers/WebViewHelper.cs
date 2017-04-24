using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace EReader.Helpers
{
    public static class WebViewHelper
    {
        public static async Task ScrollToChapter(this WebView DocumentViewer, string link)
        {
            // string alternateScript = string.Format("var els = document.querySelectorAll(\"a[href~='{0}']\");var el = els[0];el.scrollIntoView();", args.Uri.Fragment.Replace("#", ""));
            string functionString = String.Format("elmnt = document.getElementById('{0}'); scrollTo(document.body, elmnt.offsetTop, 600);", link.Replace("#", ""));
            await DocumentViewer.InvokeScriptAsync("eval", new string[] { functionString });
        }
        public static async Task ChangeFontSize(this WebView DocumentViewer, int size)
        {
            string functionString = String.Format("document.body.style.fontSize = {0};", size);
            await DocumentViewer.InvokeScriptAsync("eval", new string[] { functionString });
        }
        public static async Task ChangeTheme(this WebView DocumentViewer, string background, string foreground)
        {
            string functionString = String.Format("document.body.style.background = '{0}'; document.body.style.color = '{1}';", background, foreground);
            await DocumentViewer.InvokeScriptAsync("eval", new string[] { functionString });
        }
        public static async Task SetTheme(this WebView DocumentViewer, bool theme)
        {
            switch (theme)
            {
                case false:
                    await ChangeTheme(DocumentViewer, "#2b2b2b", "#dbdbdb");
                    (Window.Current.Content as Frame).RequestedTheme = ElementTheme.Dark;
                    break;
                case true:
                    await ChangeTheme(DocumentViewer, "#ffffff", "#1b1b1b");
                    (Window.Current.Content as Frame).RequestedTheme = ElementTheme.Light;
                    break;
            }
        }
        public static async Task IncreaseFontSize(this WebView DocumentViewer)
        {
            await DocumentViewer.InvokeScriptAsync("eval", new string[] { GetFontResizeFunction("+", "-") });
        }
        public static async Task DecreaseFontSize(this WebView DocumentViewer)
        {
            await DocumentViewer.InvokeScriptAsync("eval", new string[] { GetFontResizeFunction("-", "+") });
        }
        private static string GetFontResizeFunction(string sign, string scrollSign)
        {
            //change font size and maintain scroll position. 
            //This is tricky and not very accurate, but does the job well.
            string resizeScript = string.Format(
                           "var oldHeight = document.body.scrollHeight; var scroll = document.body.scrollTop;" +
                           "var pElements = document.querySelector(\"p\");" +
                           "var style = window.getComputedStyle(pElements, null).getPropertyValue('font-size');" +
                           "var currentSize = parseInt(style);currentSize{0}{0};document.body.style.fontSize = currentSize;", sign);
            if (Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1))
            {
                resizeScript += string.Format("if(currentSize < 17)" +
                            "scrollTo(document.body, ((scroll/oldHeight) * document.body.scrollHeight), 100);" +
                            "else scrollTo(document.body, ((scroll/oldHeight) * document.body.scrollHeight) {0} 100, 100);", scrollSign);
             } 
            else
            {
                resizeScript += string.Format("scrollTo(document.body, ((scroll/oldHeight) * document.body.scrollHeight) {0} 250, 100);", scrollSign);
            }
            return resizeScript;
        }
    }
}
