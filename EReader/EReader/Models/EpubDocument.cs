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
        public EpubDocument(StorageFile file) : base(file) { }
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
    }
}
