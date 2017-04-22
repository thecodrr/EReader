using EReader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EReader.Services
{
    public interface IEBookLibraryService
    {
        Task InsertBook(EReaderDocument document);
        Task<IEnumerable<EReaderDocument>> RetrieveBooks();
        Task<IEnumerable<EReaderDocument>> SearchBooks(string query);
        Task UpdateBook(EReaderDocument book);
    }
}
