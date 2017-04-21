using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace EReader.Helpers
{
    public static class StorageItemHelper
    {
        public static Uri ConstructApplicationUriFromStorageFile(this IStorageItem storageItem)
        {
            UriBuilder builder = new UriBuilder("ms-appdata:");
            string fullFilePath = storageItem.Path;
            string applicationURI = fullFilePath.Remove(0, fullFilePath.IndexOf("LocalState\\") + "LocalState\\".Length);
            builder.Path += "///local/";
            builder.Path += applicationURI.Replace("\\", "/");
            return builder.Uri;
        }

        public static void SaveFileAccessToken(this IStorageItem storageItem)
        {
            string token = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(storageItem);
            ApplicationDataContainer fileAccessTokenContainer = ApplicationData.Current.LocalSettings.CreateContainer("FileAccessTokenContainer", ApplicationDataCreateDisposition.Always);
            fileAccessTokenContainer.Values[storageItem.Name] = token;
        }
        public static async Task<IStorageItem> RetrieveStorageItemUsingAccessToken(string storageItemName)
        {
            ApplicationDataContainer fileAccessTokenContainer = ApplicationData.Current.LocalSettings.CreateContainer("FileAccessTokenContainer", ApplicationDataCreateDisposition.Always);
            if (fileAccessTokenContainer.Values.ContainsKey(storageItemName))
            {
                string token = fileAccessTokenContainer.Values[storageItemName].ToString();
                if(token != null && Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(token))
                   return await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetItemAsync(token);
            }
            return null;
        }
    }
}
