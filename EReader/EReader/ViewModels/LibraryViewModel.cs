//using EReader.Models;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Input;
//using Windows.Storage;
//using Windows.Storage.AccessCache;
//using Windows.Storage.Pickers;

//namespace EReader.ViewModels
//{
//    public class LibraryViewModel : ViewModelBase
//    {
//        private readonly MvvmDialogs.IDialogService dialogService;

//        private ObservableCollection<EReaderDocument> _ereaderDocuments;

//        public ObservableCollection<EReaderDocument> EReaderDocuments
//        {
//            get { return _ereaderDocuments; }
//            set { Set(ref _ereaderDocuments, value); }
//        }

//        public ICommand OpenFilePickerCommand { get; private set; }

//        public LibraryViewModel(IDialogService dialogService)
//        {
//            this.dialogService = dialogService;

//            InitData();

//            InitCommands();
//        }

//        private void InitData()
//        {
//            EReaderDocuments = new ObservableCollection<EReaderDocument>();
//        }

//        private void InitCommands()
//        {
//            OpenFilePickerCommand = new RelayCommand(OpenFilePickerAsync);
//        }

//        private async void OpenFilePickerAsync()
//        {
//            var settings = new FileOpenPickerSettings
//            {
//                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
//                FileTypeFilter = new List<string> { ".txt" }
//            };

//            IReadOnlyList<StorageFile> storageFiles = await dialogService.PickMultipleFilesAsync(settings);

//            if (storageFiles.Any())
//            {
//                foreach (var file in storageFiles)
//                {
//                    var token = StorageApplicationPermissions.FutureAccessList.Add(file);

//                    StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
//                    StorageFile folderTokens = await storageFolder.CreateFileAsync("folderTokens.txt", CreationCollisionOption.ReplaceExisting);
//                    Debug.WriteLine(storageFolder.Path);

//                    EReaderDocuments.Add(new EReaderDocument(file));

//                    await FileIO.AppendTextAsync(folderTokens, token + ",");
//                }
//            }
//        }
//    }
//}
