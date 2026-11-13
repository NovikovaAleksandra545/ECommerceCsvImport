namespace ECommerceCsvImport.Models
{
    public class ImportViewModel
    {
        public string FilePath { get; set; } = string.Empty;
        public ImportResult? Result { get; set; }

        // Какая вкладка открыта: "path" или "upload"
        public string ActiveTab { get; set; } = "path";

        // Файлы из wwwroot/csv, чтобы не вводить путь вручную
        public List<string> SampleFiles { get; set; } = new List<string>();
    }
}
