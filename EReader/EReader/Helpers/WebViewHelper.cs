using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    break;
                case true:
                    await ChangeTheme(DocumentViewer, "#ffffff", "#1b1b1b");
                    break;
            }
        }
        public static async Task IncreaseFontSize(this WebView DocumentViewer)
        {
            await DocumentViewer.InvokeScriptAsync("eval", new string[] { GetFontResizeFunction("+") });
        }
        public static async Task DecreaseFontSize(this WebView DocumentViewer)
        {
            await DocumentViewer.InvokeScriptAsync("eval", new string[] { GetFontResizeFunction("-") });
        }
        private static string GetFontResizeFunction(string sign)
        {
            //basically we do the following things:
            //1. save the scroll height of the reader.
            //2. calculate the percentage scrolled
            //3. calculate the actual scroll value
            //4. change font size
            //5. get the new scroll height
            //6. get difference by dividing new and old height and then mulitplying by 110
            //7. we scroll back to the original position. (there is a slight up/down of a few pixels).
            return string.Format(
                           "var oldHeight = document.body.scrollHeight;" +
                           "var percent = (document.documentElement.scrollTop||document.body.scrollTop) / ((document.documentElement.scrollHeight||document.body.scrollHeight) - document.documentElement.clientHeight) * 100;" +
                           "var offset = (percent / 100) * ((document.documentElement.scrollHeight||document.body.scrollHeight) - document.documentElement.clientHeight);" +
                           "var pElements = document.querySelector(\"p\");" +
                           "var style = window.getComputedStyle(pElements, null).getPropertyValue('font-size');" +
                           "var currentSize = parseInt(style);currentSize{0}{0};document.body.style.fontSize = currentSize;" +
                           "var newHeight = document.body.scrollHeight;" +
                           "var difference = newHeight / oldHeight * 110;" +
                           "scrollTo(document.body, offset {0} difference, 100);", sign);
        }
    }
}
