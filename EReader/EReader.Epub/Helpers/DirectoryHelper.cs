using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
