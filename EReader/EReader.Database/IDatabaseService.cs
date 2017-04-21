using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EReader.Database
{
	public interface IDatabaseService : IDisposable
    {
        Task InsertRecords<T>(IEnumerable<T> albums, string tableName, string primaryIndex);
        void CreateDB(string dbPath = null, bool createNew = true);
        Task InsertRecord<T>(string tableName, string primaryKey, T record);
        Task<IEnumerable<T>> GetRecords<T>(string tableName);
        T GetRecord<T>(string table, string path);
        void RemoveRecord(string tableName, string key);
        void UpdateRecords<T>(IEnumerable<T> records, string tableName, string primaryIndex);
        Task<bool> UpdateRecordAsync<T>(string tableName, string primaryKey, T record);
        Task<IEnumerable<T>> QueryRecords<T>(string tableName, string term);
        int GetRecordsCount(string tableName);
        bool CheckExists<T>(string table, string path);        
    }
}
