using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBreeze;
using Newtonsoft.Json;
using DBreeze.Utils;
using DBreeze.DataTypes;
using Windows.Storage;
using DBreeze.Objects;
using DBreeze.Utils.Async;

namespace EReader.Database
{
    public class StaticKeyValueDatabase
    {
        private static string DbPath { get; set; }       
        static DBreezeEngine db;
        public static DBreezeEngine GetDatabaseEngine(string dbPath)
        {
            if (db == null || DbPath != dbPath)
            {
                DbPath = dbPath;
                var databasePath = string.IsNullOrEmpty(dbPath) ? ApplicationData.Current.LocalFolder.Path + @"\EReaderDB" : dbPath;
                var dbConfig = new DBreezeConfiguration()
                {
                    DBreezeDataFolderName = databasePath,
                    Storage = DBreezeConfiguration.eStorage.DISK
                };
                db = new DBreezeEngine(dbConfig);
            }
            return db;
        }
        public static void DisposeDatabaseEngine()
        {
            db = null;
        }
    }
    public class KeyValueStoreDatabaseService : IDatabaseService
    {
        private string DbPath;
        DBreezeEngine engine = null;
        public KeyValueStoreDatabaseService(string dbPath = null, bool createNew = true)
        {
            CreateDB(dbPath, createNew);
        }
       
        public void CreateDB(string dbPath = null, bool createNew = true)
        {
            DbPath = dbPath;
            engine = StaticKeyValueDatabase.GetDatabaseEngine(dbPath);
            DBreeze.Utils.CustomSerializator.ByteArraySerializator = (object o) => { return JsonConvert.SerializeObject(o).To_UTF8Bytes(); };
           
            DBreeze.Utils.CustomSerializator.ByteArrayDeSerializator = (byte[] bt, Type t) => { return JsonConvert.DeserializeObject(bt.UTF8_GetString(), t); };          
        }
        public bool CheckExists<T>(string table, string path)
        {
            using (var tran = engine.GetTransaction())
            {
                var item = tran.Select<byte[], byte[]>(table, 1.ToIndex(path));//.ObjectGet<T>().Entity;
                return item.Exists;
            }
           
        }        
        public void Dispose()
        {
            if (engine != null)
                engine.Dispose();
            StaticKeyValueDatabase.DisposeDatabaseEngine();
        }

        public T GetRecord<T>(string table, string path)
        {
            try
            {
                using (var tran = engine.GetTransaction())
                {
                    return tran.Select<byte[], byte[]>(table, 1.ToIndex(path)).ObjectGet<T>().Entity;
                }
            }
            catch
            {
                return default(T);
            }
        }
        
        public int GetRecordsCount(string tableName)
        {
            try
            {
                using (var tran = engine.GetTransaction())
                {
                    var count = (int)tran.Count(tableName);
                    return count;
                }
            }
            catch
            {
                return 0;
            }
        }
        
        public Task InsertRecord<T>(string tableName, string primaryKey, T record)
        {
            return Task.Run(() =>
            {
                using (var tran = engine.GetTransaction())
                {
                    var ir = tran.ObjectInsert<T>(tableName, new DBreezeObject<T>
                    {
                        Indexes = new List<DBreezeIndex>() { new DBreezeIndex(1, primaryKey) { PrimaryIndex = true } },
                        NewEntity = true,
                        //Changes Select-Insert pattern to Insert (speeds up insert process)
                        Entity = record //Entity itself
                    },
                            true);
                    tran.Commit();
                }
            });
        }
        public Task InsertRecords<T>(IEnumerable<T> albums, string tableName, string primaryIndex)
        {
            return Task.Run(() =>
            {
                using (var tran = engine.GetTransaction())
                {
                    foreach (var record in albums)
                    {
                        var ir = tran.ObjectInsert<T>(tableName, new DBreezeObject<T>
                        {
                            Indexes = new List<DBreezeIndex>
                        {
                        new DBreezeIndex(1, primaryIndex) { PrimaryIndex = true }, //PI Primary Index
                        },

                            NewEntity = true,
                            Entity = record
                        },
                            true);
                    }
                    tran.Commit();
                }
            });
        }

        public async Task<IEnumerable<T>> QueryRecords<T>(string tableName, string term)
        {
            return await Task.Run(() =>
            {
                using (var tran = engine.GetTransaction())
                {
                    var records = tran.SelectDictionary<byte[], byte[]>(tableName);
                    var recordList = new List<T>();
                    foreach (var doc in records)
                    {
                        if (doc.Key.ToUTF8String().ToLower().Contains(term.ToLower()))
                        {
                            var key = doc.Key.ToUTF8String();
                            var val = tran.Select<byte[], byte[]>(tableName, doc.Key).ObjectGet<T>().Entity;
                            recordList.Add(val);
                        }
                    }
                    return recordList;
                }
            });
        }

        public async Task<bool> UpdateRecordAsync<T>(string tableName, string primaryKey, T record)
        {
            return await Task.Run(() =>
            {
                using (var tran = engine.GetTransaction())
                {
                    var row = tran.Select<byte[], byte[]>(tableName, 1.ToIndex(primaryKey));
                    if (row.Exists)
                    {
                        var getRecord = row.ObjectGet<T>();
                        getRecord.Entity = record;
                        getRecord.NewEntity = false;
                        getRecord.Indexes = new List<DBreezeIndex> { new DBreezeIndex(1, primaryKey) { PrimaryIndex = true } }; //PI Primary Index
                        if (tran.ObjectInsert(tableName, getRecord, true).EntityWasInserted)
                        {
                            tran.Commit();
                            return true;
                        }
                    }
                    return false;
                }
            });
        }

        public void UpdateRecords<T>(IEnumerable<T> records, string tableName, string primaryIndex)
        {
            using (var tran = engine.GetTransaction())
            {
                foreach (var data in records)
                {
                    var ord = tran.Select<byte[], byte[]>(tableName, 1.ToIndex(primaryIndex)).ObjectGet<T>();
                    ord.Entity = data;
                    ord.NewEntity = false;
                    ord.Indexes = new List<DBreezeIndex> {new DBreezeIndex(1, primaryIndex) { PrimaryIndex = true } }; //PI Primary Index
                    tran.ObjectInsert(tableName, ord, true);
                }
                tran.Commit();
            }
        }

        public void RemoveRecord(string tableName, string key)
        {
            using (var tran = engine.GetTransaction())
            {
                tran.ObjectRemove(tableName, 1.ToIndex(key));
                tran.Commit();
            }
        }
       
        public async Task<IEnumerable<T>> GetRecords<T>(string tableName)
        {
            return await Task.Run(() =>
            {
                var recordList = new List<T>();
                using (var tran = engine.GetTransaction())
                {
                    var records = tran.SelectForward<byte[], byte[]>(tableName);

                    foreach (var record in records)
                    {
                        recordList.Add(record.ObjectGet<T>().Entity);
                    }
                }
                return recordList;
            });            
        }
    }
}
