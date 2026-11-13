using ECommerceCsvImport.Models;
using ECommerceCsvImport.Services;

namespace ECommerceCsvImport.Tests
{
    public class ProductImportServiceTests
    {
        private const string Header = "sku,name,description,price,currency,category_name,stock,image_url,is_active,size,color,material";
        private const string ValidRow = "3001,Чехол-книжка,Чехол с застежкой,990,RUB,Чехлы,25,/images/t.jpg,true,M,белый,силикон";

        private readonly InMemoryProductRepository _repository = new InMemoryProductRepository();
        private readonly ProductImportService _service;

        public ProductImportServiceTests()
        {
            _service = new ProductImportService(_repository);
        }

        private static string TestFile(string name)
        {
            return Path.Combine(AppContext.BaseDirectory, "testdata", name);
        }

        private ImportResult Import(params string[] rows)
        {
            return _service.ImportFromLines(new[] { Header }.Concat(rows));
        }

        [Fact]
        public void ValidRow_IsImported()
        {
            var result = Import(ValidRow);

            Assert.Equal(1, result.TotalRows);
            Assert.Equal(1, result.ImportedCount);
            Assert.Empty(result.Errors);

            var product = _repository.GetBySku("3001");
            Assert.NotNull(product);
            Assert.Equal("Чехол-книжка", product!.Name);
            Assert.Equal(990m, product.Price);
            Assert.Equal("M", product.Size);
            Assert.Equal("белый", product.Color);
        }

        [Fact]
        public void FileNotFound_ReturnsError()
        {
            var result = _service.ImportFromCsv(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv"));

            Assert.Equal(0, result.ImportedCount);
            Assert.Contains("Файл не найден", result.Errors[0].Message);
        }

        [Fact]
        public void EmptyFile_ReturnsNoData()
        {
            var result = _service.ImportFromCsv(TestFile("empty_file.csv"));

            Assert.Equal(0, result.TotalRows);
            Assert.Contains("Данные для импорта отсутствуют", Assert.Single(result.Errors).Message);
        }

        // Проверка готовых тестовых файлов из папки testdata
        [Theory]
        [InlineData("valid_accessories.csv", 2, 0)]
        [InlineData("invalid_price.csv", 0, 1)]
        [InlineData("duplicate_sku.csv", 1, 1)]
        [InlineData("missing_color.csv", 0, 1)]
        [InlineData("invalid_size.csv", 0, 1)]
        [InlineData("bad_active.csv", 0, 1)]
        public void TestDataFile_GivesExpectedResult(string fileName, int imported, int errors)
        {
            var result = _service.ImportFromCsv(TestFile(fileName));

            Assert.Equal(imported, result.ImportedCount);
            Assert.Equal(errors, result.ErrorCount);
        }

        [Fact]
        public void WrongHeader_ReturnsError()
        {
            var result = _service.ImportFromLines(new[] { "sku,name,price", "3001,Чехол,990" });

            Assert.Equal(0, result.ImportedCount);
            Assert.Contains("Неверные заголовки CSV", Assert.Single(result.Errors).Message);
        }

        [Fact]
        public void WrongFieldCount_ReturnsError()
        {
            var result = Import("3001,Чехол-книжка");

            Assert.Contains("Неверное количество полей", Assert.Single(result.Errors).Message);
        }

        [Theory]
        [InlineData(",Чехол,,990,RUB,Чехлы,25,,true,M,белый,силикон", "Поле sku не может быть пустым")]
        [InlineData("3001,,,990,RUB,Чехлы,25,,true,M,белый,силикон", "Поле name не может быть пустым")]
        [InlineData("3001,Чехол,,-500,RUB,Чехлы,25,,true,M,белый,силикон", "Поле price должно быть числом больше 0")]
        [InlineData("3001,Чехол,,abc,RUB,Чехлы,25,,true,M,белый,силикон", "Поле price должно быть числом больше 0")]
        [InlineData("3001,Чехол,,990,RUB,Чехлы,-5,,true,M,белый,силикон", "Остаток на складе должен быть целым числом не меньше 0")]
        [InlineData("3001,Чехол,,990,RUB,Чехлы,25,,yes,M,белый,силикон", "Поле is_active должно иметь значение true, false, 1 или 0")]
        [InlineData("3001,Чехол,,990,RUB,Чехлы,25,,true,XXXL,белый,силикон", "Поле size должно быть S, M, L, XL или числом")]
        [InlineData("3001,Чехол,,990,RUB,Чехлы,25,,true,25,белый,силикон", "Поле size должно быть S, M, L, XL или числом")]
        [InlineData("3001,Чехол,,990,RUB,Чехлы,25,,true,M,,силикон", "Поле color обязательно")]
        public void InvalidRow_IsNotImported(string row, string expectedError)
        {
            var result = Import(row);

            Assert.Equal(0, result.ImportedCount);
            var error = Assert.Single(result.Errors);
            Assert.Equal(2, error.RowNumber);
            Assert.Contains(expectedError, error.Message);
        }

        [Theory]
        [InlineData("m", "M")]
        [InlineData("XL", "XL")]
        [InlineData("15", "15")]
        public void Size_LetterOrNumber_IsAccepted(string size, string expected)
        {
            Import($"3001,Чехол,,990,RUB,Чехлы,25,,true,{size},белый,силикон");

            Assert.Equal(expected, _repository.GetBySku("3001")!.Size);
        }

        [Fact]
        public void DuplicateSkuInFile_FirstProductStays()
        {
            var result = Import(ValidRow, "3001,Чехол-бампер,,1090,RUB,Чехлы,8,,true,L,серый,силикон");

            Assert.Equal(2, result.TotalRows);
            Assert.Equal(1, result.ImportedCount);
            Assert.Contains("Артикул товара уже существует", Assert.Single(result.Errors).Message);
            Assert.Equal("Чехол-книжка", _repository.GetBySku("3001")!.Name);
        }

        [Fact]
        public void RepeatedImport_SkipsExistingProducts()
        {
            Import(ValidRow);
            var second = Import(ValidRow);

            Assert.Equal(0, second.ImportedCount);
            Assert.Equal(1, _repository.Count);
        }

        [Fact]
        public void EmptyOptionalFields_UseDefaults()
        {
            Import("3011,Чехол для ноутбука,,590,,,40,,true,13,белый,");

            var product = _repository.GetBySku("3011")!;
            Assert.Equal("RUB", product.Currency);
            Assert.Equal("Без категории", product.CategoryName);
            Assert.Equal("", product.Material);
        }

        [Fact]
        public void SeveralErrorsInOneRow_AllAreReported()
        {
            var result = Import(",,Плохая строка,-10,RUB,Чехлы,-5,,yes,XXXL,,");

            Assert.Equal(0, _repository.Count);
            Assert.Equal(7, result.ErrorCount);
        }
    }
}
