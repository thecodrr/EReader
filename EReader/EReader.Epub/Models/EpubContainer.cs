using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EReader.Epub.Models
{
    [XmlRoot(ElementName = "container", Namespace = "urn:oasis:names:tc:opendocument:xmlns:container")]
    public class Container
    {
        [XmlElement(ElementName = "rootfiles", Namespace = "urn:oasis:names:tc:opendocument:xmlns:container")]
        public Rootfiles Rootfiles { get; set; }
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

    [XmlRoot(ElementName = "rootfile", Namespace = "urn:oasis:names:tc:opendocument:xmlns:container")]
    public class Rootfile
    {
        [XmlAttribute(AttributeName = "full-path")]
        public string Fullpath { get; set; }
        [XmlAttribute(AttributeName = "media-type")]
        public string Mediatype { get; set; }
    }

    [XmlRoot(ElementName = "rootfiles", Namespace = "urn:oasis:names:tc:opendocument:xmlns:container")]
    public class Rootfiles
    {
        [XmlElement(ElementName = "rootfile", Namespace = "urn:oasis:names:tc:opendocument:xmlns:container")]
        public Rootfile Rootfile { get; set; }
    }
}
