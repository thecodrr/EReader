using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EReader.Epub.Models
{
    public class Book
    {
        public List<Chapter> Chapters { get; set; }
        public Metadata Metadata { get; set; }
        public string BookStyleCSS { get; set; }
    }
}
