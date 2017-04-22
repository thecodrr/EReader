using EReader.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using EReader.MVVM;
using EReader.MVVM.Commands;
using EReader.Helpers;
using EReader.Services;
using EReader.Database;
using System.IO;

namespace EReader.ViewModels
{
    public class LibraryViewModel : ViewModelBase
    {
        private EBookLibraryService EBookLibrary { get; set; }
        private ObservableCollection<EReaderDocument> _ereaderDocuments;
        public ObservableCollection<EReaderDocument> EReaderDocuments
        {
            get { return _ereaderDocuments; }
            set { Set(ref _ereaderDocuments, value); }
        }

        public ICommand AddBookCommand { get; private set; }

        public LibraryViewModel()
        {
            InitData();
            InitCommands();
        }

        private async void InitData()
        {
            EReaderDocuments = new ObservableCollection<EReaderDocument>();
            EBookLibrary = new EBookLibraryService(new KeyValueStoreDatabaseService());
            foreach(var file in await EBookLibrary.RetrieveBooks())
            {
                file.Document = await StorageFile.GetFileFromPathAsync(file.FilePath);
                EReaderDocuments.Add(file);
            }
            //ApplicationDataContainer fileAccessTokenContainer = ApplicationData.Current.LocalSettings.CreateContainer("FileAccessTokenContainer", ApplicationDataCreateDisposition.Always);
            //foreach (var value in fileAccessTokenContainer.Values)
            //{
            //    var file = await StorageItemHelper.RetrieveStorageItemUsingAccessToken(value.Key);
            //    EReaderDocuments.Add(await EpubDocument.Create((StorageFile)file));
            //}
        }

        private void InitCommands()
        {
            AddBookCommand = new RelayCommand(AddBookAsync);
        }

        private async void AddBookAsync(object para)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.FileTypeFilter.Add(".epub");
            StorageFile eBookFile = await fileOpenPicker.PickSingleFileAsync();
            if (eBookFile != null)
            {
                eBookFile.SaveFileAccessToken();
                var eBook = await EpubDocument.Create(eBookFile);
                EReaderDocuments.Add(eBook);
                await eBook.LoadEpub(eBook, eBookFile);
                await EBookLibrary.InsertBook(eBook);
            }
        }
    }
}
