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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml;

namespace EReader.Epub
{
    /// <summary>
    /// The engine runs in 5 steps:
    /// 1. Extract EPUB Contents
    /// 2. Read eBook Metadata, TOC & CSS Files
    /// 3. Move all images to the folder where complete book's html will be.
    /// 4. Parse and combine all eBook's chapters.
    /// 5. Save the parsed result into a .html file.
    /// </summary>
    public class Engine
    {
        #region Properties
        private StorageFolder RootEpubFolder { get; set; }
        private StorageFolder OPFFolder { get; set; }
        private StorageFolder EpubTextFolder { get; set; }
        #endregion

        #region Central Methods
        public async Task<(Book epubBook, StorageFile epubFile)> ReadEpubAsync(StorageFile file, bool loadExtras)
        {
            var folder = await ExtractEpubFileAsync(file.Path);
            if (folder != null)
            {
                return await ReadEpubAsync(folder, loadExtras);
            }
            return (null, null);
        }
        public async Task<(Book epubBook, StorageFile epubFile)> ReadEpubAsync(StorageFolder rootFolder, bool loadExtras)
        {
            RootEpubFolder = rootFolder;
            var opfContent = await ReadMetadata();
            var tocContent = await ReadTOC(opfContent);
            var eBook = new Book();
            var path = DirectoryHelper.GetChapterFilePath(OPFFolder.Path, opfContent.Manifest.Item.FirstOrDefault(t => t.Mediatype == "application/xhtml+xml").Href);
            EpubTextFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path));
            var fullBookPath = Path.Combine(EpubTextFolder.Path, DirectoryHelper.GetSafeFilename(opfContent.Metadata.Title) + ".html");
            StorageFile fullBook = null;
            if (!File.Exists(fullBookPath))
            {
                await MoveAllImagesAsync(opfContent);
                eBook.BookStyleCSS = await ReadCSSFiles();
                string html = string.Format("<html xmlns=\"http://www.w3.org/1999/xhtml\"><head><style>body{{padding:20px !important;}}{0}</style></head><body>", eBook.BookStyleCSS);
                string chapters = await ParseChapterFiles(opfContent.Spine.Itemref, opfContent.Manifest.Item);
                html += await HtmlRepairer.HtmlRepairer.RepairHtml(chapters);
                html += "</body></html>";
                fullBook = await SaveBookAsync(html, opfContent.Metadata);
            }
            else
                fullBook = await StorageFile.GetFileFromPathAsync(fullBookPath);
            eBook.Chapters = tocContent.NavMap.Chapters;
            eBook.Metadata = opfContent.Metadata;
            return (eBook, fullBook);
        }

        #endregion

        #region Private Methods
        //step 1
        #region Extract Methods
        /// <summary>
        /// Step 1: Extract Epub File
        /// </summary>
        /// <param name="epubFilePath"></param>
        /// <returns></returns>
        private async Task<StorageFolder> ExtractEpubFileAsync(string epubFilePath)
        {
            return await Task.Run(async () =>
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var extractedPath = localFolder.Path + "\\" + DirectoryHelper.GetSafeFilename(Path.GetFileNameWithoutExtension(epubFilePath)).Trim();
                if (!Directory.Exists(extractedPath))
                {
                    try
                    {
                        ZipFile.ExtractToDirectory(epubFilePath, extractedPath);
                    }
                    catch (InvalidDataException)
                    {
                        return null;
                    }
                }
                return await StorageFolder.GetFolderFromPathAsync(extractedPath);
            });
        }
        #endregion
        //step 2
        #region Read Methods
        /// <summary>
        /// Step 2: Read TOC
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns>Table Of Contents of the EPUB</returns>
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
        /// <summary>
        /// Step 2: Read & Save CSS Files
        /// </summary>
        /// <returns>Minified CSS</returns>
        private async Task<string> ReadCSSFiles()
        {
            if (RootEpubFolder != null)
            {
                var query = RootEpubFolder.CreateFileQueryWithOptions(DirectoryHelper.GetQueryOptions(new string[] { ".css" }));
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
        /// <summary>
        /// Step 2: Read Metadata (OPF file).
        /// </summary>
        /// <returns>Formatted OPFDocument with all the metadata and book info.</returns>
        private async Task<OPFDocument> ReadMetadata()
        {
            XmlSerializer deserializer;
            if (RootEpubFolder != null)
            {
                var metainfFolder = await RootEpubFolder.GetFolderAsync("META-INF");
                if (metainfFolder != null)
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
        #endregion
        //step 3
        #region Move Methods
        /// <summary>
        /// Step 3: Move all images to main folder.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private async Task MoveAllImagesAsync(OPFDocument document)
        {
            foreach (var image in document.Manifest.Item.Where(t => t.Mediatype.Contains("image"))) //image can be any type so we don't specify the exact mediatype.
            {
                var imagePath = Path.Combine(OPFFolder.Path, image.Href.Replace('/', '\\'));

                if (File.Exists(imagePath))
                {
                    var imageFile = await StorageFile.GetFileFromPathAsync(imagePath);
                    var parentImageFolder = (await imageFile.GetParentAsync());
                    if (!File.Exists(Path.Combine(EpubTextFolder.Path, Path.GetFileName(imageFile.Path))))
                    {
                        await imageFile.CopyAsync(EpubTextFolder);
                        await imageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }

                }
            }
        }
        #endregion
        //step 4
        #region Parse Methods
        /// <summary>
        /// Step 4: Parse all chapter files and combine them into one html
        /// </summary>
        /// <param name="Spine"></param>
        /// <param name="Items"></param>
        /// <returns></returns>
        private async Task<string> ParseChapterFiles(List<Itemref> Spine, List<Item> Items)
        {
            string html = "";
            foreach (var page in Spine)
            {
                string pageID = page.Idref;
                string pageLink = Items.FirstOrDefault(t => t.Id == pageID).Href;

                var chapterFile = await StorageFile.GetFileFromPathAsync(DirectoryHelper.GetChapterFilePath(OPFFolder.Path, pageLink));
                // Create a new parser front-end (can be re-used)
                HtmlParser parser = new HtmlParser();
                using (FileStream fs = File.Open(chapterFile.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var document = await parser.ParseAsync(fs))
                    {       //add a <span> tag before each chapter to fix chapter navigation. This is cost-free.
                        html += string.Format("<span id='{0}-ch'/>", Path.GetFileNameWithoutExtension(chapterFile.Path));
                        //document.Body.FirstElementChild.SetAttribute("id", document.Body.Attributes["id"]?.Value);
                        html += document.Body.InnerHtml;
                    }
                }
                await chapterFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }

            return html;
        }
        #endregion
        //step 5
        #region Save Methods
        /// <summary>
        /// Step 5: Save the book into a .html file
        /// </summary>
        /// <param name="html"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private async Task<StorageFile> SaveBookAsync(string html, Metadata data)
        {
            string bookName = DirectoryHelper.GetSafeFilename(data.Title) + ".html";

            string path = Path.Combine(EpubTextFolder.Path, bookName);

            if (!File.Exists(path))
            {
                var fullBook = await EpubTextFolder.CreateFileAsync(bookName, CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(fullBook, EReader.Epub.Helpers.Minifiers.MinifyHTML(html), Windows.Storage.Streams.UnicodeEncoding.Utf8);
                return fullBook;
            }
            else
                return await StorageFile.GetFileFromPathAsync(path);
        }
        #endregion

        #endregion
    }
}
