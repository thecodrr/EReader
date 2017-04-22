using EReader.Epub;
using EReader.Helpers;
using EReader.Models;
using System;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace EReader.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DocumentView : Page
    {
        private EReaderDocument Book;
        WebView DocumentViewer;
        public DocumentView()
        {
            this.InitializeComponent();
            this.SetupTransition(new DrillInNavigationTransitionInfo());
        }
        private void InitWebView()
        {
            DocumentViewer = new WebView();
            Grid.SetRow(DocumentViewer, 1);
            DocumentViewer.Opacity = 0;
            DocumentsViewGrid.Children.Add(DocumentViewer);
        }
        private void DestroyWebView()
        {
            DocumentsViewGrid.Children.Remove(DocumentViewer);
            DocumentViewer = null;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            InitWebView();
            Book = (e.Parameter as ItemClickEventArgs).ClickedItem as EReaderDocument;
            var uri = Book.Document.ConstructApplicationUriFromStorageFile();
            BookInfoPanel.DataContext = Book;
            DocumentViewer.Navigate(uri);
            DocumentViewer.NavigationCompleted += DocumentViewer_NavigationCompleted;
            DocumentViewer.NavigationStarting += DocumentViewer_NavigationStarting;
            base.OnNavigatedTo(e);
        }

        private void DocumentViewer_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            Storyboard board = new Storyboard();
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = 0;
            animation.To = 1;
            animation.BeginTime = TimeSpan.FromSeconds(1);
            animation.Duration = TimeSpan.FromSeconds(1);
            Storyboard.SetTarget(animation, DocumentViewer);
            Storyboard.SetTargetProperty(animation, "Opacity");
            board.Children.Add(animation);
            board.Begin();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            DocumentViewer.NavigationStarting -= DocumentViewer_NavigationStarting;
            DestroyWebView();
            BookInfoPanel.DataContext = null;
            GC.Collect();
            base.OnNavigatingFrom(e);
        }
        private async void DocumentViewer_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            args.Cancel = true;
            string functionString = "function scrollTo(a,b,c){if(!(c<=0)){var d=b-a.scrollTop,e=d/c*10;setTimeout(function(){a.scrollTop=a.scrollTop+e,a.scrollTop!==b&&scrollTo(a,b,c-10)},10)}}";
            // string alternateScript = string.Format("var els = document.querySelectorAll(\"a[href~='{0}']\");var el = els[0];el.scrollIntoView();", args.Uri.Fragment.Replace("#", ""));
            functionString += String.Format("elmnt = document.getElementById('{0}'); scrollTo(document.body, elmnt.offsetTop, 600);", args.Uri.Fragment.Replace("#", ""));
            var res = await DocumentViewer.InvokeScriptAsync("eval", new string[] { functionString });
        }
    }
}
