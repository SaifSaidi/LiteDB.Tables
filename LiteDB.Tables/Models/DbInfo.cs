
namespace LiteDB.Tables.Models
{
    public class DbInfo
    {
        public string? ConnectionString { get; set; } = string.Empty;
        public int UserVersion {get;set;}
    }
}
