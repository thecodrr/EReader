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
        private XmlSerializer Deserializer { get; set; }
        #endregion

        #region Central Methods
        /// <summary>
        /// Reads only the metadata (contents.opf) of the ebook.
        /// </summary>
        /// <param name="epubfilePath">The epub file path to retrieve metadata of.</param>
        /// <returns>Metadata of the ebook.</returns>
        public async Task<Metadata> ReadMetadataOnlyAsync(string epubfilePath)
        {
            RootEpubFolder = await ExtractEpubFileAsync(epubfilePath);            
            return (await ReadMetadata()).Metadata;
        }
        /// <summary>
        /// Read Epub File. This extracts the file to a folder in the ApplicationData.LocalFolder and then starts reading.
        /// </summary>
        /// <param name="file">The Epub File</param>
        /// <returns>A tuple containing the Book and the EpubFile. We need both to properly navigate to the new html file.</returns>
        public async Task<(Book epubBook, StorageFile epubFile)> ReadEpubAsync(StorageFile file)
        {
            //get the folder the epub was extracted to. step 1
            var folder = await ExtractEpubFileAsync(file.Path);

            if (folder != null)
            {
                //start reading epub.
                return await ReadEpubAsync(folder);
            }
            return (null, null);
        }

        /// <summary>
        /// This is the point where everything meets.
        /// </summary>
        /// <param name="rootFolder">The folder where epub content is.</param>
        /// <returns>A tuple containing the Book and the EpubFile.</returns>
        public async Task<(Book epubBook, StorageFile epubFile)> ReadEpubAsync(StorageFolder rootFolder)
        {
            //set the RootEpubFolder property. We will need it elsewhere.
            RootEpubFolder = rootFolder;

            //get opf content. step 2 part 1
            var opfContent = await ReadMetadata();

            //get toc. step 2 part 2
            var tocContent = await ReadTOC(opfContent);

            var eBook = new Book();

            //alright. this is complex. Let me explain.
            //1. We use the opf metadata to get the link to first chapter file.
            //2. We use that chapter file's path to get the main text folder where all the html is.
            //3. Afterwards, we just use the main text folder to retrieve the full book html file (if it exists).
            var path = DirectoryHelper.GetChapterFilePath(OPFFolder.Path, opfContent.Manifest.Item.FirstOrDefault(t => t.Mediatype == "application/xhtml+xml").Href);
            EpubTextFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path));
            var fullBookPath = Path.Combine(EpubTextFolder.Path, DirectoryHelper.GetSafeFilename(opfContent.Metadata.Title) + ".html");

            //set the variable to null. We use it later in multiple places but return it in one place.
            StorageFile fullBook = null;

            //make sure the full book doesn't already exist.
            if (!File.Exists(fullBookPath))
            {
                //just move the images. step 3
                await MoveAllImagesAsync(opfContent);

                //get the css. //step 2 part 3
                eBook.BookStyleCSS = await ReadCSSFiles();

                //the is the html that holds the styles and all the other required structure.
                string html = string.Format("<html xmlns=\"http://www.w3.org/1999/xhtml\"><head><style>body{{padding:20px !important;}}{0}</style></head><body>", eBook.BookStyleCSS);

                //get the chapters. step 4
                string chapters = await ParseChapterFiles(opfContent.Spine.Itemref, opfContent.Manifest.Item);

                //repair the html (links and images). (Is this a step too?)
                html += await HtmlRepairer.HtmlRepairer.RepairHtml(chapters);

                //close the html. We are done.
                html += "</body></html>";

                //just go on and save. step 5.
                fullBook = await SaveBookAsync(html, opfContent.Metadata);
            }
            else
            {
                //book exists. REUSE!
                fullBook = await StorageFile.GetFileFromPathAsync(fullBookPath);
            }
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
                        //TODO: Report error and reason.
                        //some archives return "Unexpected End Of Archive Reached". 
                        //This is to catch that kind of exceptions and handle them.
                        //So the app doesn't crash.
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
            Deserializer = new XmlSerializer(typeof(TOC));
            var tocFilePath = Path.Combine(OPFFolder.Path, metadata.Manifest.Item.FirstOrDefault(t => t.Mediatype == "application/x-dtbncx+xml").Href);
            var tocFile = await StorageFile.GetFileFromPathAsync(tocFilePath);
            using (var stream = await tocFile.OpenStreamForReadAsync())
            {
                return (TOC)Deserializer.Deserialize(stream);
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
                //query for all css files
                var query = RootEpubFolder.CreateFileQueryWithOptions(DirectoryHelper.GetQueryOptions(new string[] { ".css" }));

                //variable to hold all the css.
                string css = "";
                foreach (var file in await query.GetFilesAsync())
                {
                    //TODO: use a different method for reading text i.e. Filestreams
                    css += await FileIO.ReadTextAsync(file);

                    //Delete css file, we no longer need it.
                    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }

                //minify CSS to decrease size (just removes whitespaces).
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
            if (RootEpubFolder != null)
            {
                var metainfFolder = await RootEpubFolder.GetFolderAsync("META-INF");
                if (metainfFolder != null)
                {
                    //get container.xml file
                    StorageFile containerFile = await StorageFile.GetFileFromPathAsync(Path.Combine(metainfFolder.Path, "container.xml"));

                    //create a new XmlDeserializer to deserialize Container Content
                    Deserializer = new XmlSerializer(typeof(Container));
                    using (var containerStream = await containerFile.OpenStreamForReadAsync())
                    {
                        var containerXML = (Container)Deserializer.Deserialize(containerStream);

                        //get contents.opf file from epub directory
                        var opfFile = await StorageFile.GetFileFromPathAsync(Path.Combine(RootEpubFolder.Path, containerXML.Rootfiles.Rootfile.Fullpath.Replace('/', '\\')));

                        //set OPFFolder for later use.
                        OPFFolder = await opfFile.GetParentAsync();

                        //Reset the deserializer with new content type
                        Deserializer = new XmlSerializer(typeof(OPFDocument));

                        using (var opfStream = await opfFile.OpenStreamForReadAsync())
                        {
                            return (OPFDocument)Deserializer.Deserialize(opfStream);
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
            //image can be any type so we don't specify the exact mediatype.
            foreach (var image in document.Manifest.Item.Where(t => t.Mediatype.Contains("image")))
            {
                //create image path from data we already have.
                var imagePath = Path.Combine(OPFFolder.Path, image.Href.Replace('/', '\\'));

                //we don't want to move a non-existing file.
                if (File.Exists(imagePath))
                {
                    //we are sure the file exists. Create a StorageFile out of it.
                    var imageFile = await StorageFile.GetFileFromPathAsync(imagePath);
                    
                    //make sure the image doesn't already exists.
                    if (!File.Exists(Path.Combine(EpubTextFolder.Path, Path.GetFileName(imageFile.Path))))
                    {
                        await imageFile.CopyAsync(EpubTextFolder);

                        //delete file. We no longer need it.
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
        /// <remarks>
        /// We need to parse according to the Spine element in the opf file.
        /// Otherwise, all chapters will be sorted out weirdly.
        /// Hence, to move according to the Spine, we need to compare it with
        /// Manifest Element of opf file which contains all the info about all
        /// the files in the epub.
        /// </remarks>
        /// <param name="Spine"></param>
        /// <param name="Items"></param>
        /// <returns>The parsed HTML which is the COMPLETE book.</returns>
        private async Task<string> ParseChapterFiles(List<Itemref> Spine, List<Item> Items)
        {
            //variable to hold our html in.
            string html = "";

            foreach (var chapter in Spine)
            {
                //get chapterID from Spine
                string pageID = chapter.Idref;

                //get the path to the chapter by comparing pageID with ItemId.
                string pageLink = Items.FirstOrDefault(t => t.Id == pageID).Href;

                //make a StorageFile out of the gotten link.
                var chapterFile = await StorageFile.GetFileFromPathAsync(DirectoryHelper.GetChapterFilePath(OPFFolder.Path, pageLink));
                
                // Create a new parser front-end (can be re-used)
                HtmlParser parser = new HtmlParser();

                //use FileStream as it is faster and uses less memory. (still doubt that but even so)
                using (FileStream fs = File.Open(chapterFile.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var document = await parser.ParseAsync(fs))
                    {       
                        //add a <span> tag before each chapter to fix chapter navigation. This is a hack, a good one.
                        html += string.Format("<span id='{0}-ch'/>", Path.GetFileNameWithoutExtension(chapterFile.Path));
                        
                        //we only need the body's innerhtml
                        html += document.Body.InnerHtml;
                    }
                }

                //delete chapter file, we no longer need it.
                await chapterFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }

            //return the compiled html.
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
            //get bookname and append the extension to it.
            string bookName = DirectoryHelper.GetSafeFilename(data.Title) + ".html";

            //create Path out of the bookname. We will use this to create the final book html file.
            string path = Path.Combine(EpubTextFolder.Path, bookName);

            //make sure it doesn't already exist
            if (!File.Exists(path))
            {
                //try to create file. I say try because...well...we never know.
                var fullBook = await EpubTextFolder.CreateFileAsync(bookName, CreationCollisionOption.OpenIfExists);

                //Write all the html to the file.
                //TODO: Use a better way to write to file.
                await FileIO.WriteTextAsync(fullBook, EReader.Epub.Helpers.Minifiers.MinifyHTML(html), Windows.Storage.Streams.UnicodeEncoding.Utf8);

                //return the new complete book file.
                return fullBook;
            }
            else
            {
                //the book exists. Just reuse it.
                return await StorageFile.GetFileFromPathAsync(path);
            }
        }
        #endregion

        #endregion
    }
}
