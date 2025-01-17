
namespace LiteDB.Tables.Models
{
    public class TableData
    {
        public List<string> Columns { get; set; } = [];
        public List<Dictionary<string, BsonValue>> Rows { get; set; } = [];
        public string? ConnectionString { get; set; }
        public string? LastModified { get; set; }  // Last modified/open time
    }

}
