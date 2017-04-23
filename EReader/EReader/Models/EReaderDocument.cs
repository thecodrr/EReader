using EReader.Epub.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace EReader.Models
{
    public class EReaderDocument : ObservableObject
    {
        string title;
        string author;
        string cover;

        public string Title
        {
            get => title;
            set => Set(ref title, value);
        }

        public string Author
        {
            get => author;
            set => Set(ref author, value);
        }

        public string CoverImageSource
        {
            get => cover;
            set => Set(ref cover, value);
        }

        public string Tag { get; set; }

        public string FilePath { get; set; }

        [JsonIgnore]
        public StorageFile Document { get; set; }

        public string Description { get; set; }        
        public double LastReadPosition { get; set; }
        bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => Set(ref isLoading, value);
        }
        double loadProgress;
        public double LoadProgress
        {
            get => loadProgress;
            set => Set(ref loadProgress, value);
        }
        public List<Chapter> Chapters { get; set; }

        public EReaderDocument()
        {

        }
        public EReaderDocument(StorageFile file)
        {
            FilePath = file.Path;
            Document = file;
            Author = "";
            CoverImageSource = "";
        }        
    }
}
