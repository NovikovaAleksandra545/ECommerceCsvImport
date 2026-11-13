namespace ECommerceCsvImport.Models
{
    public class ImportViewModel
    {
        public string FilePath { get; set; } = string.Empty;
        public ImportResult? Result { get; set; }

        // Файлы из wwwroot/csv, чтобы не вводить путь вручную
        public List<string> SampleFiles { get; set; } = new List<string>();
    }
}
