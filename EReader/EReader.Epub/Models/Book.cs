using System.Collections.Generic;

namespace EReader.Epub.Models
{
    public class Book
    {
        public List<Chapter> Chapters { get; set; }
        public Metadata Metadata { get; set; }
        public string BookStyleCSS { get; set; }
        public string CoverImage { get; set; }
    }
}
