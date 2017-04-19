using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EReader.Epub.Models
{
    [XmlRoot(ElementName = "contributor", Namespace = "http://purl.org/dc/elements/1.1/")]
    public class Contributor
    {
        [XmlAttribute(AttributeName = "file-as", Namespace = "http://www.idpf.org/2007/opf")]
        public string Fileas { get; set; }
        [XmlAttribute(AttributeName = "role", Namespace = "http://www.idpf.org/2007/opf")]
        public string Role { get; set; }
    }

    [XmlRoot(ElementName = "creator", Namespace = "http://purl.org/dc/elements/1.1/")]
    public class Creator
    {
        [XmlAttribute(AttributeName = "file-as", Namespace = "http://www.idpf.org/2007/opf")]
        public string Fileas { get; set; }
        [XmlAttribute(AttributeName = "role", Namespace = "http://www.idpf.org/2007/opf")]
        public string Role { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "date", Namespace = "http://purl.org/dc/elements/1.1/")]
    public class Date
    {
        [XmlAttribute(AttributeName = "event", Namespace = "http://www.idpf.org/2007/opf")]
        public string Event { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "guide", Namespace = "http://www.idpf.org/2007/opf")]
    public class Guide
    {
        [XmlElement(ElementName = "reference", Namespace = "http://www.idpf.org/2007/opf")]
        public List<Reference> Reference { get; set; }
    }

    [XmlRoot(ElementName = "identifier", Namespace = "http://purl.org/dc/elements/1.1/")]
    public class Identifier
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "scheme", Namespace = "http://www.idpf.org/2007/opf")]
        public string Scheme { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "item", Namespace = "http://www.idpf.org/2007/opf")]
    public class Item
    {
        [XmlAttribute(AttributeName = "href")]
        public string Href { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "media-type")]
        public string Mediatype { get; set; }
    }

    [XmlRoot(ElementName = "itemref", Namespace = "http://www.idpf.org/2007/opf")]
    public class Itemref
    {
        [XmlAttribute(AttributeName = "idref")]
        public string Idref { get; set; }
        [XmlAttribute(AttributeName = "linear")]
        public string Linear { get; set; }
    }

    [XmlRoot(ElementName = "manifest", Namespace = "http://www.idpf.org/2007/opf")]
    public class Manifest
    {
        [XmlElement(ElementName = "item", Namespace = "http://www.idpf.org/2007/opf")]
        public List<Item> Item { get; set; }
    }    

    [XmlRoot(ElementName = "metadata", Namespace = "http://www.idpf.org/2007/opf")]
    public class Metadata
    {
        [XmlElement(ElementName = "contributor", Namespace = "http://purl.org/dc/elements/1.1/")]
        public Contributor Contributor { get; set; }
        [XmlElement(ElementName = "coverage", Namespace = "http://purl.org/dc/elements/1.1/")]
        public string Coverage { get; set; }
        [XmlElement(ElementName = "creator", Namespace = "http://purl.org/dc/elements/1.1/")]
        public Creator Creator { get; set; }
        [XmlElement(ElementName = "date", Namespace = "http://purl.org/dc/elements/1.1/")]
        public Date Date { get; set; }
        [XmlAttribute(AttributeName = "dc", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Dc { get; set; }
        [XmlElement(ElementName = "description", Namespace = "http://purl.org/dc/elements/1.1/")]
        public string Description { get; set; }
        [XmlElement(ElementName = "identifier", Namespace = "http://purl.org/dc/elements/1.1/")]
        public Identifier Identifier { get; set; }
        [XmlElement(ElementName = "language", Namespace = "http://purl.org/dc/elements/1.1/")]
        public string Language { get; set; }
        [XmlAttribute(AttributeName = "opf", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Opf { get; set; }
        [XmlElement(ElementName = "publisher", Namespace = "http://purl.org/dc/elements/1.1/")]
        public string Publisher { get; set; }
        [XmlElement(ElementName = "rights", Namespace = "http://purl.org/dc/elements/1.1/")]
        public string Rights { get; set; }
        [XmlElement(ElementName = "source", Namespace = "http://purl.org/dc/elements/1.1/")]
        public string Source { get; set; }
        [XmlElement(ElementName = "subject", Namespace = "http://purl.org/dc/elements/1.1/")]
        public string Subject { get; set; }
        [XmlElement(ElementName = "title", Namespace = "http://purl.org/dc/elements/1.1/")]
        public string Title { get; set; }
        [XmlElement(ElementName = "type", Namespace = "http://purl.org/dc/elements/1.1/")]
        public string Type { get; set; }
    }

    [XmlRoot(ElementName = "package", Namespace = "http://www.idpf.org/2007/opf")]
    public class OPFDocument
    {
        [XmlElement(ElementName = "guide", Namespace = "http://www.idpf.org/2007/opf")]
        public Guide Guide { get; set; }
        [XmlElement(ElementName = "manifest", Namespace = "http://www.idpf.org/2007/opf")]
        public Manifest Manifest { get; set; }
        [XmlElement(ElementName = "metadata", Namespace = "http://www.idpf.org/2007/opf")]
        public Metadata Metadata { get; set; }
        [XmlElement(ElementName = "spine", Namespace = "http://www.idpf.org/2007/opf")]
        public Spine Spine { get; set; }
    }

    [XmlRoot(ElementName = "reference", Namespace = "http://www.idpf.org/2007/opf")]
    public class Reference
    {
        [XmlAttribute(AttributeName = "href")]
        public string Href { get; set; }
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
    }

    [XmlRoot(ElementName = "spine", Namespace = "http://www.idpf.org/2007/opf")]
    public class Spine
    {
        [XmlElement(ElementName = "itemref", Namespace = "http://www.idpf.org/2007/opf")]
        public List<Itemref> Itemref { get; set; }
        [XmlAttribute(AttributeName = "toc")]
        public string Toc { get; set; }
    }
}
