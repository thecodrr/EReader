using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web;

namespace EReader.Common
{
    public sealed class StreamUriResolver : IUriToStreamResolver
    {
        /// <summary>
        /// The entry point for resolving a Uri to a stream.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public IAsyncOperation<IInputStream> UriToStreamAsync(Uri uri)
        {
            if (uri == null)
            {
                throw new Exception();
            }
            string path = uri.AbsolutePath;
            // Because of the signature of this method, it can't use await, so we 
            // call into a separate helper method that can use the C# await pattern.
            return GetContent(path).AsAsyncOperation();
        }
        /// <summary>
        /// Helper that maps the path to package content and resolves the Uri
        /// Uses the C# await pattern to coordinate async operations
        /// </summary>
        private async Task<IInputStream> GetContent(string path)
        {
            // We use a package folder as the source, but the same principle should apply
            // when supplying content from other locations
            try
            {
                // Don't use "ms-appdata:///" on the scheme string, because inside the path
                // will contain "/local/MyFolderOnLocal/index.html"
                string scheme = "ms-appdata://" + path;

                Uri localUri = new Uri(scheme);
                StorageFile f = await StorageFile.GetFileFromApplicationUriAsync(localUri);
                IRandomAccessStream stream = await f.OpenAsync(FileAccessMode.Read);
                return stream.GetInputStreamAt(0);
            }
            catch (Exception) { throw new Exception("Invalid path"); }
        }
    }
}
