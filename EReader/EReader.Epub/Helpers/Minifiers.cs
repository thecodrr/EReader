using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZetaHtmlCompressor;
namespace EReader.Epub.Helpers
{
    public class Minifiers
    {
        public static string MinifyCSS(string body)
        {
            body = Regex.Replace(body, @"[a-zA-Z]+#", "#");
            body = Regex.Replace(body, @"[\n\r]+\s*", string.Empty);
            body = Regex.Replace(body, @"\s+", " ");
            body = Regex.Replace(body, @"\s?([:,;{}])\s?", "$1");
            body = body.Replace(";}", "}");
            body = Regex.Replace(body, @"([\s:]0)(px|pt|%|em)", "$1");

            // Remove comments from CSS
            body = Regex.Replace(body, @"/\*[\d\D]*?\*/", string.Empty);

            return body;
        }
        public static string MinifyHTML(string html)
        {
            ZetaHtmlCompressor.HtmlContentCompressor compressor = new HtmlContentCompressor();
            return compressor.Compress(html);
        }
    }
}
