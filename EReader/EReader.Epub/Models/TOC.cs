using System.Collections.Generic;
using System.Xml.Serialization;

namespace EReader.Epub.Models
{
    [XmlRoot(ElementName = "docTitle", Namespace = "http://www.daisy.org/z3986/2005/ncx/")]
    public class DocTitle
    {
        [XmlElement(ElementName = "text", Namespace = "http://www.daisy.org/z3986/2005/ncx/")]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "navLabel", Namespace = "http://www.daisy.org/z3986/2005/ncx/")]
    public class NavLabel
    {
        [XmlElement(ElementName = "text", Namespace = "http://www.daisy.org/z3986/2005/ncx/")]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "content", Namespace = "http://www.daisy.org/z3986/2005/ncx/")]
    public class Content
    {
        [XmlAttribute(AttributeName = "src")]
        public string Src { get; set; }
    }

    [XmlRoot(ElementName = "navPoint", Namespace = "http://www.daisy.org/z3986/2005/ncx/")]
    public class Chapter
    {
        [XmlElement(ElementName = "navLabel", Namespace = "http://www.daisy.org/z3986/2005/ncx/")]
        public NavLabel NavLabel { get; set; }
        [XmlElement(ElementName = "content", Namespace = "http://www.daisy.org/z3986/2005/ncx/")]
        public Content Content { get; set; }
        public string ChapterLink { get { return HtmlRepairer.HtmlRepairer.RepairLink(Content.Src); } }
        public string ChapterTitle { get { return NavLabel.Text; } }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "playOrder")]
        public string ChapterNo { get; set; }
        public string HtmlContent { get; set; }
        [XmlElement(ElementName = "navPoint", Namespace = "http://www.daisy.org/z3986/2005/ncx/")]
        public List<Chapter> Subchapters { get; set; }
    }

    [XmlRoot(ElementName = "navMap", Namespace = "http://www.daisy.org/z3986/2005/ncx/")]
    public class NavMap
    {
        [XmlElement(ElementName = "navPoint", Namespace = "http://www.daisy.org/z3986/2005/ncx/")]
        public List<Chapter> Chapters { get; set; }
    }

    [XmlRoot(ElementName = "ncx", Namespace = "http://www.daisy.org/z3986/2005/ncx/")]
    public class TOC
    {
        [XmlElement(ElementName = "docTitle", Namespace = "http://www.daisy.org/z3986/2005/ncx/")]
        public DocTitle DocTitle { get; set; }
        [XmlElement(ElementName = "navMap", Namespace = "http://www.daisy.org/z3986/2005/ncx/")]
        public NavMap NavMap { get; set; }
    }
}
