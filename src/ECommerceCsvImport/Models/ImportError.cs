namespace ECommerceCsvImport.Models
{
    // Ошибка импорта. RowNumber = 0, если ошибка относится ко всему файлу
    public class ImportError
    {
        public int RowNumber { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
