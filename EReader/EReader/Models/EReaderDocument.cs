using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace EReader.Models
{
    class EReaderDocument
    {
        private string _title;

        private string _author;

        private string _coverImageSource;

        private string _tag;

        private string _filePath;

        private FileInfo _fileInfo;

        private StorageFile _document;

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public string Author
        {
            get { return _author; }
            set { _author = value; }
        }

        public string CoverImageSource
        {
            get { return _coverImageSource; }
            set { _coverImageSource = value; }
        }

        public string Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; }
        }

        public FileInfo MetaData
        {
            get { return _fileInfo; }
            set { _fileInfo = value; }
        }

        public StorageFile Document
        {
            get { return _document; }
            set { _document = value; }
        }

        public EReaderDocument(StorageFile file)
        {
            FilePath = file.Path;
            MetaData = new FileInfo(FilePath);
            Document = file;
            Title = MetaData.Name;
            Author = "";
            CoverImageSource = "";
        }
        
    }
}
