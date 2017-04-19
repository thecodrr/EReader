using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace EReader.Helpers
{
    public static class StorageFileHelper
    {
        public static string ConstructApplicationUriFromStorageFile(this StorageFile file)
        {
            string prefix = "ms-appdata:///local/";
            string fullFilePath = file.Path;
            string applicationURI = fullFilePath.Remove(0, fullFilePath.IndexOf("LocalState\\") + "LocalState\\".Length);
            return prefix + applicationURI.Replace("\\", "//");
        }
    }
}
