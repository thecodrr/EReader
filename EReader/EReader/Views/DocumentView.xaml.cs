using EReader.Helpers;
using System;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
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
            OpenFile();
        }
        async void OpenFile()
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".epub");
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                EReader.Epub.Engine engine = new Epub.Engine();
                var book = await engine.ReadEpubAsync(file, false);
                var uri = book.epubFile.ConstructApplicationUriFromStorageFile();
                DocumentViewer.Navigate(uri);
                GC.Collect();
            }
            DocumentViewer.NavigationStarting += DocumentViewer_NavigationStarting;

            //FolderPicker picker = new FolderPicker();
            //picker.FileTypeFilter.Add(".epub");
            //var folder = await picker.PickSingleFolderAsync();
            //if (folder != null)
            //{
            //    foreach (var file in await folder.GetFilesAsync())
            //    {
            //        if (file.FileType.Contains("epub"))
            //        {
            //            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
            //            EReader.Epub.Engine engine = new Epub.Engine();
            //            var book = await engine.ReadEpubAsync(file, false);
            //            //var uri = book.epubFile.ConstructApplicationUriFromStorageFile();
            //            //DocumentViewer.Navigate(new Uri(uri));
            //        }
            //    }
            //}
            //DocumentViewer.NavigationStarting += DocumentViewer_NavigationStarting;
        }


        private async void DocumentViewer_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            args.Cancel = true;
           // string alternateScript = string.Format("var els = document.querySelectorAll(\"a[href~='{0}']\");var el = els[0];el.scrollIntoView();", args.Uri.Fragment.Replace("#", ""));
            string functionString = String.Format("document.getElementById('{0}').scrollIntoView();", args.Uri.Fragment.Replace("#",""));
            var res = await DocumentViewer.InvokeScriptAsync("eval", new string[] { functionString });
        }
    }
}
