using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace EReader.Models
{
    public class EReaderDocument
    {
        public string Title { get; set; }

        public string Author { get; set; }

        public string CoverImageSource { get; set; }

        public string Tag { get; set; }

        public string FilePath { get; set; }

        public StorageFile Document { get; set; }

        public string Description { get; set; }        

        public EReaderDocument(StorageFile file)
        {
            FilePath = file.Path;
            Document = file;
            Author = "";
            CoverImageSource = "";
        }        
    }
}
