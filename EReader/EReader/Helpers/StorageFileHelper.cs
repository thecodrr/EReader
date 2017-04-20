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
        public static Uri ConstructApplicationUriFromStorageFile(this StorageFile file)
        {
            UriBuilder builder = new UriBuilder("ms-appdata:");
            string fullFilePath = file.Path;
            string applicationURI = fullFilePath.Remove(0, fullFilePath.IndexOf("LocalState\\") + "LocalState\\".Length);
            builder.Path += "///local/";
            builder.Path += applicationURI.Replace("\\", "/");
            return builder.Uri;
        }
    }
}
