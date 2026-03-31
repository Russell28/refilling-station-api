namespace RefillingStation.Api.Common.Import
{
    public class ImportError
    {
        public int RowNumber { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
