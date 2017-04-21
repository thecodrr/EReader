using EReader.Epub;
using EReader.Helpers;
using EReader.Models;
using System;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace EReader.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DocumentView : Page
    {
        public DocumentView()
        {
            this.InitializeComponent();
            
        }      
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var book = (e.Parameter as ItemClickEventArgs).ClickedItem as EReaderDocument;
            var uri = book.Document.ConstructApplicationUriFromStorageFile();
            DocumentViewer.Navigate(uri);
            base.OnNavigatedTo(e);
        }
        private async void DocumentViewer_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            args.Cancel = true;
            string functionString = "function scrollTo(a,b,c){if(!(c<=0)){var d=b-a.scrollTop,e=d/c*10;setTimeout(function(){a.scrollTop=a.scrollTop+e,a.scrollTop!==b&&scrollTo(a,b,c-10)},10)}}";
            // string alternateScript = string.Format("var els = document.querySelectorAll(\"a[href~='{0}']\");var el = els[0];el.scrollIntoView();", args.Uri.Fragment.Replace("#", ""));
            functionString += String.Format("elmnt = document.getElementById('{0}'); scrollTo(document.body, elmnt.offsetTop, 600);", args.Uri.Fragment.Replace("#",""));
            var res = await DocumentViewer.InvokeScriptAsync("eval", new string[] { functionString });
        }
    }
}
