using System.Text;
using ECommerceCsvImport.Models;
using ECommerceCsvImport.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceCsvImport.Controllers
{
    public class ProductsController : Controller
    {
        private const string DefaultFilePath = "/csv/sample_products.csv";
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 МБ

        private readonly IProductRepository _repository;
        private readonly IProductImportService _importService;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(IProductRepository repository, IProductImportService importService,
            IWebHostEnvironment environment)
        {
            _repository = repository;
            _importService = importService;
            _environment = environment;
        }

        // Список товаров с поиском, фильтрами и сортировкой
        [HttpGet]
        public IActionResult Index(string? search, string? category, string? size, string? sort)
        {
            var all = _repository.GetAll();
            var model = new ProductsIndexViewModel
            {
                Products = Filter(all, search, category, size, sort),
                AllCount = all.Count,
                Search = search,
                Category = category,
                Size = size,
                Sort = sort,
                Categories = all.Select(p => p.CategoryName).Distinct().OrderBy(c => c).ToList(),
                Sizes = all.Select(p => p.Size).Distinct().OrderBy(s => s.Length).ThenBy(s => s).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Details(string id)
        {
            var product = _repository.GetBySku(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View(CreateImportModel(DefaultFilePath, null));
        }

        // Импорт по пути: /csv/file.csv (папка wwwroot) или полный путь C:\data\file.csv
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Import(string? filePath)
        {
            ImportResult result;

            if (string.IsNullOrWhiteSpace(filePath))
                result = ErrorResult("Укажите путь к CSV-файлу");
            else
                result = _importService.ImportFromCsv(ResolvePath(filePath));

            return View(CreateImportModel(filePath ?? "", result));
        }

        // Импорт файла, выбранного в браузере
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upload(IFormFile? file)
        {
            ImportResult result;

            if (file == null)
                result = ErrorResult("Выберите CSV-файл");
            else if (file.Length == 0)
                result = ErrorResult("Данные для импорта отсутствуют");
            else if (file.Length > MaxFileSize)
                result = ErrorResult("Файл слишком большой (максимум 5 МБ)");
            else
            {
                using var stream = file.OpenReadStream();
                result = _importService.ImportFromStream(stream, file.FileName);
            }

            return View("Import", CreateImportModel(DefaultFilePath, result));
        }

        // Выгрузка товаров в CSV (в том же формате, что и для импорта)
        [HttpGet]
        public IActionResult Export(string? search, string? category, string? size, string? sort)
        {
            var products = Filter(_repository.GetAll(), search, category, size, sort);

            var csv = new StringBuilder();
            csv.AppendLine(string.Join(",", ProductImportService.RequiredColumns));
            foreach (var p in products)
            {
                csv.AppendLine(string.Join(",", new[]
                {
                    p.Sku, p.Name, p.Description, p.Price.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    p.Currency, p.CategoryName, p.StockQuantity.ToString(), p.ImageUrl,
                    p.IsActive ? "true" : "false", p.Size, p.Color, p.Material
                }.Select(EscapeCsv)));
            }

            // BOM нужен, чтобы Excel правильно открыл кириллицу
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
            return File(bytes, "text/csv", $"products_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            _repository.Clear();
            TempData["Message"] = "Все товары удалены из памяти";
            return RedirectToAction(nameof(Index));
        }

        private static List<Product> Filter(IEnumerable<Product> products, string? search, string? category,
            string? size, string? sort)
        {
            if (!string.IsNullOrWhiteSpace(search))
                products = products.Where(p => p.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase)
                                               || p.Sku.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(category))
                products = products.Where(p => p.CategoryName == category);

            if (!string.IsNullOrEmpty(size))
                products = products.Where(p => p.Size == size);

            products = sort switch
            {
                "name" => products.OrderBy(p => p.Name),
                "price_asc" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                "stock" => products.OrderByDescending(p => p.StockQuantity),
                _ => products
            };

            return products.ToList();
        }

        private string ResolvePath(string input)
        {
            string path = input.Trim().Trim('"');

            // Сначала проверяем путь как есть (полный путь на диске).
            // На Linux "/csv/file.csv" тоже считается полным путем, поэтому если такого файла нет - ищем в wwwroot.
            if (Path.IsPathFullyQualified(path) && System.IO.File.Exists(path))
                return path;

            string inWebRoot = Path.Combine(_environment.WebRootPath, path.TrimStart('~', '/', '\\'));
            if (System.IO.File.Exists(inWebRoot) || !Path.IsPathFullyQualified(path))
                return inWebRoot;

            return path;
        }

        private ImportViewModel CreateImportModel(string filePath, ImportResult? result)
        {
            string csvFolder = Path.Combine(_environment.WebRootPath, "csv");

            return new ImportViewModel
            {
                FilePath = filePath,
                Result = result,
                SampleFiles = Directory.Exists(csvFolder)
                    ? Directory.GetFiles(csvFolder, "*.csv").Select(f => "/csv/" + Path.GetFileName(f)).OrderBy(f => f).ToList()
                    : new List<string>()
            };
        }

        private static ImportResult ErrorResult(string message)
        {
            var result = new ImportResult();
            result.Errors.Add(new ImportError { RowNumber = 0, Message = message });
            return result;
        }

        private static string EscapeCsv(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains(';'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";

            return value;
        }
    }
}
