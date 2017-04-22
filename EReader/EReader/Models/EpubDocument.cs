using EReader.Epub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace EReader.Models
{
    public class EpubDocument : EReaderDocument
    {
        /// <summary>
        /// Private Constructor. Why? Because we use factory pattern to initialize EpubDocument.
        /// To avoid other classes calling this construction, we make it private.
        /// </summary>
        /// <param name="file"></param>
        private EpubDocument(StorageFile file) : base(file) { }

        /// <summary>
        /// This is actually the constructor or to be specific an async method that acts
        /// like a constructor. 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async static Task<EpubDocument> Create(StorageFile file)
        {
            var epubDoc = new EpubDocument(file);           
            return epubDoc;
        }
        public async Task LoadEpub(EpubDocument epubDoc, StorageFile file)
        {
            epubDoc.IsLoading = true;
            EReader.Epub.Engine engine = new Epub.Engine();
            var progress = new Common.Progress<int>((prog, progStatus) => { epubDoc.LoadProgress = prog; epubDoc.Author = progStatus; });
            var book = await engine.ReadEpubAsync(file, progress);
            epubDoc.CoverImageSource = book.epubBook.CoverImage;
            epubDoc.Title = book.epubBook.Metadata.Title;
            epubDoc.Author = book.epubBook.Metadata.Creator.Text;
            epubDoc.FilePath = book.epubFile.Path;
            epubDoc.Document = await StorageFile.GetFileFromPathAsync(epubDoc.FilePath);
            epubDoc.Chapters = book.epubBook.Chapters;
            epubDoc.Description = book.epubBook.Metadata.Description;
            epubDoc.IsLoading = false;
        }
        public List<Chapter> Chapters { get; set; }
    }
}
