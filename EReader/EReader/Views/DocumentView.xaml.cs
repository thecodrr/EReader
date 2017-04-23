using EReader.Database;
using EReader.Epub;
using EReader.Epub.Models;
using EReader.Helpers;
using EReader.Models;
using EReader.Services;
using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace EReader.Views
{
    /// <summary>
    ///  TODO: Add TOC
    /// </summary>
    public sealed partial class DocumentView : Page
    {
        private EReaderDocument Book;
        DispatcherTimer saveReadingProgress;
        EBookLibraryService LibraryService;      
        double oldProgress;

        /// <summary>
        /// The Poor Constructor
        /// </summary>
        public DocumentView()
        {
            this.InitializeComponent();
            this.SetupTransition(new DrillInNavigationTransitionInfo());
        }
        
        /// <summary>
        /// Destroy the WebView. We no longer need it.
        /// </summary>
        private void DestroyWebView()
        {
            DocumentsViewGrid.Children.Remove(DocumentViewer);
            DocumentViewer = null;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //there seems to be a bug in the NavigateToAction
            //because the parameter is not set or if it is,
            //it doesn't take bindings very nicely.
            //This is the workaround.
            Book = (e.Parameter as ItemClickEventArgs).ClickedItem as EReaderDocument;
            tocList.ItemsSource = Book.Chapters ?? null;
            
            //construct URI
            var uri = Book.Document.ConstructApplicationUriFromStorageFile();
            
            //set the datacontext before we load the book.
            BookInfoPanel.DataContext = Book;
            
            //make a new uri for loading into webview
            var newUri = DocumentViewer.BuildLocalStreamUri(Book.Title, uri.AbsolutePath);
            DocumentViewer.NavigateToLocalStreamUri(newUri, new EReader.Common.StreamUriResolver());
          
            //register events.
            DocumentViewer.NavigationStarting += DocumentViewer_NavigationStarting;
            DocumentViewer.LoadCompleted += DocumentViewer_LoadCompleted;
            DocumentViewer.ScriptNotify += DocumentViewer_ScriptNotify;

            //register timer
            saveReadingProgress = new DispatcherTimer();
            saveReadingProgress.Interval = TimeSpan.FromSeconds(2);
            saveReadingProgress.Tick += SaveReadingProgress_Tick;

            //initiate library service
            LibraryService = new EBookLibraryService(new KeyValueStoreDatabaseService());
            base.OnNavigatedTo(e);
        }
        private async void SaveReadingProgress_Tick(object sender, object e)
        {
            //do not update if the reader is not scrolling further.
            //save memory and disk operations.
            if (oldProgress == Book.LastReadPosition)
                return;

            //save progress :)
            await SaveReadingProgress().ConfigureAwait(false);

            //save the progress in a variable to be reused again.
            oldProgress = Book.LastReadPosition;
        }
        private async Task LoadReadingProgress()
        {
            //we need not scroll to 0 because we are already there ;)
            if (Book.LastReadPosition > 0)
            {
                //the scroll function.
                //we reverse calculate the scroll offset; from percentage to actual value.
                string functionString = "var offset = ({0} / 100) * ((document.documentElement.scrollHeight||document.body.scrollHeight) - document.documentElement.clientHeight); scrollTo(document.body, offset, 500);";

                //invoke script in browser to scroll to the last read position. This will take 500 milliseconds.
                //TODO: add an option here to specify how long should the scroll take.
                await DocumentViewer.InvokeScriptAsync("eval", new string[] { string.Format(functionString, Book.LastReadPosition) });
            }
        }
        private async Task SaveReadingProgress()
        {
            await LibraryService.UpdateBook(Book);
        }
        private void DocumentViewer_ScriptNotify(object sender, NotifyEventArgs e)
        {
            if(e.Value != null)
            {
                double position = Convert.ToDouble(e.Value) * 100;
                readingProgress.Value = position;
                Book.LastReadPosition = readingProgress.Value;
            }
        }

        private async void DocumentViewer_LoadCompleted(object sender, NavigationEventArgs e)
        {
            //hide the progress
            progressIndicator.Visibility = Visibility.Collapsed;

            //the book has loaded so start the save timer.
            saveReadingProgress.Start();            

            //load the reading progress.
            await LoadReadingProgress();
        }
        
        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //save progress before navigating away.
            await SaveReadingProgress();

            //dump the events.
            DocumentViewer.NavigationStarting -= DocumentViewer_NavigationStarting;
            DocumentViewer.LoadCompleted -= DocumentViewer_LoadCompleted;
            DocumentViewer.ScriptNotify -= DocumentViewer_ScriptNotify;
            saveReadingProgress.Tick -= SaveReadingProgress_Tick;

            //free up memory.
            DestroyWebView();            
            BookInfoPanel.DataContext = null;

            //run the GC to collect garbage.
            GC.Collect();
            base.OnNavigatingFrom(e);
        }
        private async void DocumentViewer_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            //cancel navigation because we do not want to navigate to another page.
            args.Cancel = true;
            await ScrollToChapter(args.Uri.Fragment);
        }
        private async Task ScrollToChapter(string link)
        {
            // string alternateScript = string.Format("var els = document.querySelectorAll(\"a[href~='{0}']\");var el = els[0];el.scrollIntoView();", args.Uri.Fragment.Replace("#", ""));
            string functionString = String.Format("elmnt = document.getElementById('{0}'); scrollTo(document.body, elmnt.offsetTop, 600);", link.Replace("#", ""));
            var res = await DocumentViewer.InvokeScriptAsync("eval", new string[] { functionString });
        }
        private async void tocList_ItemClick(object sender, ItemClickEventArgs e)
        {
            await ScrollToChapter((e.ClickedItem as Chapter).ChapterLink);
        }
    }
}
