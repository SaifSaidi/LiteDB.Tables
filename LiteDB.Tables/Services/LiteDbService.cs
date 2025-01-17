using LiteDB;
using LiteDB.Tables.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;


namespace LiteDB.Tables.Services
{ 
    public class LiteDbService
    {
        private readonly ILiteDatabase _db;
        private readonly string ConnectionString = string.Empty;
        public LiteDbService(IOptions<LiteDBTablesConfig> options)
        {
            ConnectionString = options.Value.ConnectionString;
            if(string.IsNullOrEmpty(ConnectionString))
            {
                throw new ArgumentException("ConnectionString is required");
            }
            _db = new LiteDatabase(ConnectionString); 
        }

        public DbInfo GetDbInfo() => new()
        {
            ConnectionString = ConnectionString,
            UserVersion = _db.UserVersion
        };


        public IEnumerable<TableInfo> GetTables()
        { 
            return _db.GetCollectionNames()
                .Select(name => new TableInfo {
                Name = name,
                ConnectionString = ConnectionString,

            });
        }

        public TableData GetTableData(string tableName)
        {
            var collection = _db.GetCollection(tableName);
            var documents = collection.FindAll().ToList();

            var columns = new List<string>();
            var rows = new List<Dictionary<string, BsonValue>>();

            foreach (var doc in documents)
            {
                var row = new Dictionary<string, BsonValue>();
                foreach (var item in doc.AsDocument)
                {
                    if (!columns.Contains(item.Key))
                    {
                        columns.Add($"{item.Key}");
                    }
                    row[item.Key] = JsonSerializer.Serialize(item.Value) ;
                    
                }
                rows.Add(row);
            } 

             var lastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            return new TableData
            {
                Columns = columns,
                Rows = rows,
                ConnectionString = ConnectionString,
                LastModified = lastModified
            };
        }
       
    }

}