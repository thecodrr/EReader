using EReader.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace EReader.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LibraryView : Page
    {
        internal List<EReaderDocument> EReaderDocuments { get; private set; }

        public LibraryView()
        {
            this.InitializeComponent();
        }

        private async void AddNewFileButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var token = StorageApplicationPermissions.FutureAccessList.Add(file);

                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile folderTokens = await storageFolder.CreateFileAsync("folderTokens.txt", CreationCollisionOption.ReplaceExisting);
                Debug.WriteLine(storageFolder.Path);

                EReaderDocuments.Add(new EReaderDocument(file));

                await FileIO.WriteTextAsync(folderTokens, token);
            }
        }
    }
}
