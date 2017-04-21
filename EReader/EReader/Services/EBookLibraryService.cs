using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EReader.Models;
using EReader.Database;

namespace EReader.Services
{
    public class EBookLibraryService : IEBookLibraryService
    {
        private IDatabaseService DatabaseService { get; set; }
        public EBookLibraryService(IDatabaseService service)
        {
            DatabaseService = service;
        }
        public async Task InsertBook(EReaderDocument document)
        {
            await DatabaseService.InsertRecord("Ebooks", document.Title + document.Author, document);
        }

        public async Task<IEnumerable<EReaderDocument>> RetrieveBooks()
        {
            return await DatabaseService.GetRecords<EReaderDocument>("Ebooks");
        }
        
        public async Task<IEnumerable<EReaderDocument>> SearchBooks(string query)
        {
            return await DatabaseService.QueryRecords<EReaderDocument>("Ebooks", query);
        }
    }
}
