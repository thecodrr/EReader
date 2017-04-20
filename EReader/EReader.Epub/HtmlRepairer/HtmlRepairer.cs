using AngleSharp.Dom;
using AngleSharp.Parser.Html;
using EReader.Epub.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EReader.Epub.HtmlRepairer
{
    class HtmlRepairer
    {
        public static async Task<string> RepairHtml(string html)
        {
            var parser = new HtmlParser();
            //Just get the DOM representation
            using (var document = await parser.ParseAsync(html))
            {
                foreach (var anchor in document.QuerySelectorAll("a"))
                {
                    if (anchor.Attributes["href"] == null && string.IsNullOrEmpty(anchor.InnerHtml))
                        anchor.Remove();
                    else if (anchor.Attributes["href"] != null)
                    {
                        string repairedLink = RepairLink(anchor.Attributes["href"].Value);
                        if (!string.IsNullOrEmpty(repairedLink))
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
        }
        public static string RepairLink(string link)
        {
            if (string.IsNullOrEmpty(link))
                return null;
            if (link.StartsWith("#")) //means it is a path to an id which we don't want to repair
                return link;
            if (link.Contains("#")) //is a potential id
            {
                link = link.Remove(link.IndexOf("#"));
            }
            if (link.EndsWith(".html") || link.EndsWith(".xml") || link.EndsWith(".xhtml")) // repair needed here 
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
    }
}
