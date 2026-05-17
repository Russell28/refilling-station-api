
namespace RefillingStation.Application.DTOs.Imports
{
    public class ImportResult
    {
        public int TotalRows { get; set; }
        public int InsertedRows { get; set; }
        public int FailedRows { get; set; }
        public List<ImportError> Errors { get; set; } = new();
    }
}
