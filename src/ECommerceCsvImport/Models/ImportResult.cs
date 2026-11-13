namespace ECommerceCsvImport.Models
{
    // Итог импорта, который показывается на странице
    public class ImportResult
    {
        public string FileName { get; set; } = string.Empty;
        public int TotalRows { get; set; }
        public int ImportedCount { get; set; }
        public List<ImportError> Errors { get; set; } = new List<ImportError>();

        public int ErrorCount => Errors.Count;
    }
}
