using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;

namespace EReader.Epub.Helpers
{
    public class DirectoryHelper
    {
        public static QueryOptions GetQueryOptions(IList<string> filters)
        {
            QueryOptions options = new QueryOptions(CommonFileQuery.OrderByName, filters);
            options.FolderDepth = FolderDepth.Deep;
            return options;
        }
        public static string GetChapterFilePath(string opfFolderPath, string chapterLink)
        {
            var path = Path.Combine(opfFolderPath, System.Net.WebUtility.HtmlDecode(chapterLink).Replace("/", "\\"));
            return Uri.UnescapeDataString(path);
        }
        public static string GetSafeFilename(string filename)
        {
            return string.Join("", filename.Split(Path.GetInvalidFileNameChars()));
        }
        public static async Task<StorageFile> GetFullPathFromRelativePath(StorageFolder rootFolder, string relativePath)
        {
            if (relativePath.StartsWith("../"))
            {
                var folderUp = await rootFolder.GetParentAsync();
                string path = relativePath.Remove(0,2).Replace("/","\\");
                string fullPath = folderUp.Path + path;
                return await StorageFile.GetFileFromPathAsync(fullPath); 
            }
            return null;
        }
    }
}
