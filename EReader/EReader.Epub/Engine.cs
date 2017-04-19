using AngleSharp.Dom;
using AngleSharp.Parser.Html;
using EReader.Epub.Helpers;
using EReader.Epub.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Search;

namespace EReader.Epub
{
    public class Engine
    {
        private StorageFolder RootEpubFolder { get; set; }
        private StorageFolder OPFFolder { get; set; }
        public StorageFolder EpubTextFolder { get; set; }
        private async Task<StorageFolder> ExtractEpubFileAsync(string epubFilePath)
        {
            return await Task.Run(async () =>
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                if (!Directory.Exists(localFolder.Path + "\\" + Path.GetFileNameWithoutExtension(epubFilePath)))
                    ZipFile.ExtractToDirectory(epubFilePath, localFolder.Path + "\\" + Path.GetFileNameWithoutExtension(epubFilePath).Trim());
                return await StorageFolder.GetFolderFromPathAsync(localFolder.Path + "\\" + Path.GetFileNameWithoutExtension(epubFilePath).Trim());
            });
        }
        private async Task<TOC> ReadTOC(OPFDocument metadata)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(TOC));
            var tocFilePath = Path.Combine(OPFFolder.Path, metadata.Manifest.Item.FirstOrDefault(t => t.Mediatype == "application/x-dtbncx+xml").Href);
            var tocFile = await StorageFile.GetFileFromPathAsync(tocFilePath);
            using (var stream = await tocFile.OpenStreamForReadAsync())
            {
                return (TOC)deserializer.Deserialize(stream);
            }
        }
        private async Task<string> ReadCSSFiles()
        {
            if (RootEpubFolder != null)
            {
                var query = RootEpubFolder.CreateFileQueryWithOptions(DirectoryHelper.GetQueryOptions(new string[] { ".css"}));
                string css = "";
                foreach (var file in await query.GetFilesAsync())
                {
                    css += await FileIO.ReadTextAsync(file);
                    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
                return Minifiers.MinifyCSS(css);
            }
            return null;
        }
        private async Task<OPFDocument> ReadMetadata()
        {
            XmlSerializer deserializer;
            if (RootEpubFolder != null)
            {
                var metainfFolder = await RootEpubFolder.GetFolderAsync("META-INF");
                if(metainfFolder != null)
                {
                    Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(metainfFolder);
                    var containerXPath = Path.Combine(metainfFolder.Path, "container.xml");
                    StorageFile containerFile = await StorageFile.GetFileFromPathAsync(containerXPath);
                    deserializer = new XmlSerializer(typeof(Container));
                    using (var containerStream = await containerFile.OpenStreamForReadAsync())
                    {
                        var containerXML = (Container)deserializer.Deserialize(containerStream);
                        var opfFile = await StorageFile.GetFileFromPathAsync(Path.Combine(RootEpubFolder.Path, containerXML.Rootfiles.Rootfile.Fullpath.Replace('/', '\\')));
                        OPFFolder = await opfFile.GetParentAsync();
                        deserializer = new XmlSerializer(typeof(OPFDocument));
                        using (var opfStream = await opfFile.OpenStreamForReadAsync())
                        {
                            return (OPFDocument)deserializer.Deserialize(opfStream);
                        }
                    }
                }
            }
            return null;
        }
        public async Task<(Book epubBook, StorageFile epubFile)> ReadEpubAsync(StorageFile file, bool loadExtras)
        {
            var folder = await ExtractEpubFileAsync(file.Path);
            return await ReadEpubAsync(folder, loadExtras);
        }
        private async Task<(Book epubBook, StorageFile epubFile)> ReadEpubAsync(StorageFolder rootFolder, bool loadExtras)
        {
            RootEpubFolder = rootFolder;
            var opfContent = await ReadMetadata();
            var tocContent = await ReadTOC(opfContent);
            var eBook = new Book();
            var path = GetChapterFilePath(tocContent.NavMap.Chapters[0].ChapterLink);
            EpubTextFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path));
            var fullBookPath = Path.Combine(EpubTextFolder.Path, GetSafeFilename(opfContent.Metadata.Title) + ".html");
            StorageFile fullBook = null;
            if (!File.Exists(fullBookPath))
            {
                eBook.BookStyleCSS = await ReadCSSFiles();
                string html = string.Format("<html xmlns=\"http://www.w3.org/1999/xhtml\"><head><style>body{{margin:20px !important;}}{0}</style></head><body>", eBook.BookStyleCSS);
                html += await ParseChapterFiles(opfContent.Spine.Itemref, opfContent.Manifest.Item);
                html += "</body></html>";
                fullBook = await SaveBookAsync(html, opfContent.Metadata);
            }
            else
                fullBook = await StorageFile.GetFileFromPathAsync(fullBookPath);
            eBook.Chapters = tocContent.NavMap.Chapters;
            eBook.Metadata = opfContent.Metadata;
            return (eBook, fullBook);            
        }
        private async Task<string> ParseChapterFiles(List<Itemref> Spine, List<Item> Items)
        {
            string html = "";
            foreach (var page in Spine)
            {
                string pageID = page.Idref;
                string pageLink = Items.FirstOrDefault(t => t.Id == pageID).Href;

                var chapterFile = await StorageFile.GetFileFromPathAsync(GetChapterFilePath(pageLink));
                // Create a new parser front-end (can be re-used)
                var parser = new HtmlParser();
                //Just get the DOM representation
                var document = await parser.ParseAsync(await FileIO.ReadTextAsync(chapterFile));
                html += document.Body.InnerHtml;
                await chapterFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                GC.Collect();
            }
            return html;
        }
        private string GetChapterFilePath(string chapterLink)
        {
            var path = Path.Combine(OPFFolder.Path, System.Net.WebUtility.HtmlDecode(chapterLink).Replace("/", "\\"));
            if (path.Contains("#"))
            {
                path = path.Remove(path.IndexOf('#') - 0);
            }
            return Uri.UnescapeDataString(path);
        }
        private async Task<StorageFile> SaveBookAsync(string html, Metadata data)
        {
            if (!File.Exists(Path.Combine(EpubTextFolder.Path, GetSafeFilename(data.Title) + ".html")))
            {
                var fullBook = await EpubTextFolder.CreateFileAsync(GetSafeFilename(data.Title) + ".html", CreationCollisionOption.FailIfExists);
                await FileIO.WriteTextAsync(fullBook, EReader.Epub.Helpers.Minifiers.MinifyHTML(html), Windows.Storage.Streams.UnicodeEncoding.Utf8);
                return fullBook;
            }
            else
                return await StorageFile.GetFileFromPathAsync(Path.Combine(EpubTextFolder.Path, GetSafeFilename(data.Title) + ".html"));
        }     
        
        private string GetSafeFilename(string filename)
        {
            return string.Join("", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
