using System.Globalization;
using System.Text;
using ECommerceCsvImport.Models;

namespace ECommerceCsvImport.Services
{
    /// <summary>
    /// Импорт товаров (вариант 3) из CSV: чтение файла, проверка заголовков,
    /// валидация строк и добавление корректных товаров в репозиторий.
    /// Строки с ошибками и дубликаты артикулов пропускаются.
    /// </summary>
    public class ProductImportService : IProductImportService
    {
        // Обязательные колонки CSV
        public static readonly string[] RequiredColumns =
        {
            "sku", "name", "description", "price", "currency", "category_name",
            "stock", "image_url", "is_active", "size", "color", "material"
        };

        private const string DefaultCurrency = "RUB";
        private const string DefaultCategory = "Без категории";

        // Допустимые размеры: буквенные или диагональ устройства в дюймах
        private static readonly string[] LetterSizes = { "S", "M", "L", "XL" };
        private const int MinNumericSize = 10;
        private const int MaxNumericSize = 20;

        private readonly IProductRepository _repository;
        private readonly ILogger<ProductImportService>? _logger;

        public ProductImportService(IProductRepository repository, ILogger<ProductImportService>? logger = null)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger;
        }

        // Импорт из файла на диске
        public ImportResult ImportFromCsv(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return FileError("Файл не найден", filePath);

            if (!filePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return FileError("Можно загружать только файлы .csv", filePath);

            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                var result = ImportFromLines(ReadLines(bytes));
                result.FileName = Path.GetFileName(filePath);
                return result;
            }
            catch (IOException ex)
            {
                _logger?.LogError(ex, "Ошибка чтения файла {Path}", filePath);
                return FileError("Не удалось прочитать файл: " + ex.Message, filePath);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger?.LogError(ex, "Нет доступа к файлу {Path}", filePath);
                return FileError("Нет доступа к файлу", filePath);
            }
        }

        // Импорт файла, загруженного через браузер
        public ImportResult ImportFromStream(Stream stream, string fileName)
        {
            if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return FileError("Можно загружать только файлы .csv", fileName);

            using var memory = new MemoryStream();
            stream.CopyTo(memory);

            var result = ImportFromLines(ReadLines(memory.ToArray()));
            result.FileName = Path.GetFileName(fileName);
            return result;
        }

        // Основная логика импорта (отдельный метод, чтобы удобно тестировать без файлов)
        public ImportResult ImportFromLines(IEnumerable<string> lines)
        {
            var result = new ImportResult();
            var allLines = lines.ToList();

            // заголовок - первая непустая строка
            int headerIndex = allLines.FindIndex(l => !string.IsNullOrWhiteSpace(l));
            if (headerIndex < 0)
            {
                result.Errors.Add(new ImportError { RowNumber = 0, Message = "Данные для импорта отсутствуют" });
                return result;
            }

            string headerLine = allLines[headerIndex].TrimStart('﻿');
            char separator = CsvLineParser.DetectSeparator(headerLine);
            var headers = CsvLineParser.Parse(headerLine, separator);

            // колонка -> индекс (без учета регистра)
            var columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Count; i++)
                columns.TryAdd(headers[i], i);

            var missing = RequiredColumns.Where(c => !columns.ContainsKey(c)).ToList();
            if (missing.Count > 0)
            {
                result.Errors.Add(new ImportError
                {
                    RowNumber = headerIndex + 1,
                    Message = "Неверные заголовки CSV, нет колонок: " + string.Join(", ", missing)
                });
                return result;
            }

            // артикулы, добавленные из этого файла, - чтобы ловить дубликаты внутри файла
            var importedSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = headerIndex + 1; i < allLines.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(allLines[i]))
                    continue;

                result.TotalRows++;
                int rowNumber = i + 1;
                var values = CsvLineParser.Parse(allLines[i], separator);

                if (values.Count != headers.Count)
                {
                    result.Errors.Add(new ImportError
                    {
                        RowNumber = rowNumber,
                        Message = $"Неверное количество полей в строке (ожидалось {headers.Count}, получено {values.Count})"
                    });
                    continue;
                }

                string Get(string column) => values[columns[column]];

                var rowErrors = new List<string>();
                var product = ValidateRow(Get, rowErrors);

                if (rowErrors.Count > 0)
                {
                    foreach (var message in rowErrors)
                        result.Errors.Add(new ImportError { RowNumber = rowNumber, Message = message });
                    continue;
                }

                if (importedSkus.Contains(product.Sku) || _repository.GetBySku(product.Sku) != null)
                {
                    result.Errors.Add(new ImportError
                    {
                        RowNumber = rowNumber,
                        Message = "Артикул товара уже существует: " + product.Sku
                    });
                    continue;
                }

                _repository.Add(product);
                importedSkus.Add(product.Sku);
                result.ImportedCount++;
            }

            if (result.TotalRows == 0 && result.ErrorCount == 0)
                result.Errors.Add(new ImportError { RowNumber = 0, Message = "Данные для импорта отсутствуют" });

            _logger?.LogInformation("Импорт: строк {Total}, добавлено {Imported}, ошибок {Errors}",
                result.TotalRows, result.ImportedCount, result.ErrorCount);

            return result;
        }

        // Проверка полей одной строки. Ошибки собираются все сразу, а не до первой.
        private static Product ValidateRow(Func<string, string> get, List<string> errors)
        {
            string sku = get("sku");
            if (sku == "")
                errors.Add("Поле sku не может быть пустым");

            string name = get("name");
            if (name == "")
                errors.Add("Поле name не может быть пустым");

            // цена может быть записана как 1990.50 или 1990,50
            string priceText = get("price").Replace(',', '.');
            if (!decimal.TryParse(priceText, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price) || price <= 0)
                errors.Add("Поле price должно быть числом больше 0");

            if (!int.TryParse(get("stock"), out int stock) || stock < 0)
                errors.Add("Остаток на складе должен быть целым числом не меньше 0");

            bool isActive = false;
            switch (get("is_active").ToLower())
            {
                case "true":
                case "1":
                    isActive = true;
                    break;
                case "false":
                case "0":
                    isActive = false;
                    break;
                default:
                    errors.Add("Поле is_active должно иметь значение true, false, 1 или 0");
                    break;
            }

            string size = NormalizeSize(get("size"));
            if (size == "")
                errors.Add($"Поле size должно быть S, M, L, XL или числом от {MinNumericSize} до {MaxNumericSize}");

            string color = get("color");
            if (color == "")
                errors.Add("Поле color обязательно для заполнения");

            string currency = get("currency");
            string category = get("category_name");

            return new Product
            {
                Sku = sku,
                Name = name,
                Description = get("description"),
                Price = price,
                Currency = currency == "" ? DefaultCurrency : currency.ToUpper(),
                CategoryName = category == "" ? DefaultCategory : category,
                StockQuantity = stock,
                ImageUrl = get("image_url"),
                IsActive = isActive,
                Size = size,
                Color = color,
                Material = get("material")
            };
        }

        // Возвращает размер в нормальном виде (M, XL, 15) или пустую строку, если размер неверный
        private static string NormalizeSize(string value)
        {
            string upper = value.ToUpper();
            if (LetterSizes.Contains(upper))
                return upper;

            if (int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int number)
                && number >= MinNumericSize && number <= MaxNumericSize)
                return number.ToString();

            return "";
        }

        // Чтение строк файла. Если файл не в UTF-8 (например, сохранен из Excel), читаем как Windows-1251
        private static List<string> ReadLines(byte[] bytes)
        {
            string text;
            try
            {
                text = new UTF8Encoding(false, true).GetString(bytes);
            }
            catch (DecoderFallbackException)
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                text = Encoding.GetEncoding(1251).GetString(bytes);
            }

            return text.TrimStart('﻿')
                .Split('\n')
                .Select(l => l.TrimEnd('\r'))
                .ToList();
        }

        private static ImportResult FileError(string message, string? path)
        {
            var result = new ImportResult { FileName = Path.GetFileName(path ?? "") };
            result.Errors.Add(new ImportError { RowNumber = 0, Message = message });
            return result;
        }
    }
}
