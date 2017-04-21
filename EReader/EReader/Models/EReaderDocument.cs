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

        protected string Title { get; set; }

        protected string Author { get; set; }

        protected string CoverImageSource { get; set; }

        protected string Tag { get; set; }

        protected string FilePath { get; set; }

        protected StorageFile Document { get; set; }

        protected string Description { get; set; }        

        public EReaderDocument(StorageFile file)
        {
            FilePath = file.Path;
            Document = file;
            Author = "";
            CoverImageSource = "";
        }        
    }
}
