using ECommerceCsvImport.Models;

namespace ECommerceCsvImport.Services
{
    public interface IProductImportService
    {
        ImportResult ImportFromCsv(string filePath);
        ImportResult ImportFromStream(Stream stream, string fileName);
        ImportResult ImportFromLines(IEnumerable<string> lines);
    }
}
