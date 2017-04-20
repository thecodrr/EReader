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
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Search;

namespace EReader.Epub
{
    public class Engine
    {
        #region Properties
        private StorageFolder RootEpubFolder { get; set; }
        private StorageFolder OPFFolder { get; set; }
        private StorageFolder EpubTextFolder { get; set; }
        #endregion

        #region Read Methods
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

        #region Parse Methods
        private async Task<string> ParseChapterFiles(List<Itemref> Spine, List<Item> Items)
        {
            string html = "";
            foreach (var page in Spine)
            {
                string pageID = page.Idref;
                string pageLink = Items.FirstOrDefault(t => t.Id == pageID).Href;

                var chapterFile = await StorageFile.GetFileFromPathAsync(DirectoryHelper.GetChapterFilePath(OPFFolder.Path, pageLink));
                // Create a new parser front-end (can be re-used)
                XmlDocument document = new XmlDocument();
                document.LoadXml(await FileIO.ReadTextAsync(chapterFile));
                //add a <span> tag before each chapter to fix chapter navigation. This is cost-free.
                html += string.Format("<span id='{0}-ch'/>", Path.GetFileNameWithoutExtension(chapterFile.Path));
                html += document.GetElementsByTagName("body")[0].InnerXml;
                await chapterFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
          
            return html;
        }
        #endregion

        #region Extract Methods
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
        #endregion

        #region Book Read Methods
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
            var path = DirectoryHelper.GetChapterFilePath(OPFFolder.Path, tocContent.NavMap.Chapters[0].ChapterLink);
            EpubTextFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path));
            var fullBookPath = Path.Combine(EpubTextFolder.Path, DirectoryHelper.GetSafeFilename(opfContent.Metadata.Title) + ".html");
            StorageFile fullBook = null;
            if (!File.Exists(fullBookPath))
            {
                await MoveAllImagesAsync(opfContent);
                eBook.BookStyleCSS = await ReadCSSFiles();
                string html = string.Format("<html xmlns=\"http://www.w3.org/1999/xhtml\"><head><style>body{{padding:20px !important;}}{0}</style></head><body>", eBook.BookStyleCSS);
                string chapters = await ParseChapterFiles(opfContent.Spine.Itemref, opfContent.Manifest.Item);
                html += await RepairHtml(chapters);
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

        #region Save Methods
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

        private async Task<string> RepairHtml(string html)
        {
            var parser = new HtmlParser();
            //Just get the DOM representation
            var document = await parser.ParseAsync(html);
            int index = 0;
            foreach (var anchor in document.QuerySelectorAll("a"))
            {
                if (anchor.Attributes["href"] == null && string.IsNullOrEmpty(anchor.InnerHtml))
                    anchor.Remove();
                else if (anchor.Attributes["href"] != null)
                {
                    string repairedLink = RepairLink(anchor.Attributes["href"].Value);
                    if(!string.IsNullOrEmpty(repairedLink))
                        anchor.SetAttribute("href", repairedLink);
                }
            }
            foreach (var image in document.QuerySelectorAll("img, image"))
            {
                if (image.Attributes["src"] != null)
                    image.SetAttribute("src", image.Attributes["src"].Value.Remove(0, image.Attributes["src"].Value.LastIndexOf('/') + 1));
                else if (image.Attributes["src"] == null && image.Attributes["href"] != null) //is svg, parse differently
                {
                    image.SetAttribute("href", image.Attributes["href"].Value.Remove(0, image.Attributes["href"].Value.LastIndexOf('/') + 1));
                }
            }
            return document.Body.InnerHtml;
        }
        public string RepairLink(string link)
        {
            if (string.IsNullOrEmpty(link))
                return null;
            if (link.StartsWith("#")) //means it is a path to an id which we don't want to repair
                return link;
            if (link.Contains("#")) //is a potential id
            {
                return link.Remove(0, link.IndexOf("#"));
            }
            else if (link.EndsWith(".html") || link.EndsWith(".xml") || link.EndsWith(".xhtml")) // repair needed here 
            {
                //hack for relative paths
                link = link.Replace("\\", "/");
                if (link.StartsWith(".") || link.Contains("/"))
                    link = link.Remove(0, link.LastIndexOf("/"));
                //make an id link
                return "#" + DirectoryHelper.GetSafeFilename(link.Substring(0, link.LastIndexOf("."))) + "-ch"; //we are sure there will be no space in the link as spaces are not accepted in html linking.
            }
            
            return link; //nothing worked so just return the original link
        }
        private string FindOrSetHeaderID(IElement header, int index)
        {
            if (header.Attributes["id"] == null)
            {
                var id = Regex.Replace(header.TextContent.ToLower(), "[^a-zA-Z0-9]", "");
                header.SetAttribute("id", "ch-" + index + "-" + id.Replace(" ",""));
            }
            return header.Attributes["id"].Value;
        }
        private async Task MoveAllImagesAsync(OPFDocument document)
        {
            foreach (var image in document.Manifest.Item.Where(t => t.Mediatype.Contains("image"))) //image can be any type so we don't specify the exact mediatype.
            {
                var imagePath = Path.Combine(OPFFolder.Path, image.Href.Replace('/','\\'));
               
                if (File.Exists(imagePath))
                {
                    var imageFile = await StorageFile.GetFileFromPathAsync(imagePath);
                    if (OPFFolder.Path != (await imageFile.GetParentAsync()).Path)
                    {
                        await imageFile.CopyAsync(EpubTextFolder);
                        await imageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }
                }
            }
        }
    }
}
