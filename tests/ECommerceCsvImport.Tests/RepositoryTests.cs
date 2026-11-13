using ECommerceCsvImport.Models;
using ECommerceCsvImport.Services;

namespace ECommerceCsvImport.Tests
{
    public class RepositoryTests
    {
        [Fact]
        public void Add_AddsProduct_GetAllContainsIt()
        {
            var repository = new InMemoryProductRepository();

            repository.Add(new Product { Sku = "3001", Name = "Чехол-книжка", Price = 990m });

            var all = repository.GetAll();
            Assert.Single(all);
            Assert.Equal("3001", all[0].Sku);
            Assert.Equal(1, repository.Count);
        }

        [Fact]
        public void GetBySku_IgnoresCaseAndSpaces()
        {
            var repository = new InMemoryProductRepository();
            repository.Add(new Product { Sku = "AB-3001", Name = "Чехол" });

            Assert.NotNull(repository.GetBySku("AB-3001"));
            Assert.NotNull(repository.GetBySku(" ab-3001 "));
        }

        [Fact]
        public void NewRepository_IsEmpty()
        {
            var repository = new InMemoryProductRepository();

            Assert.Equal(0, repository.Count);
            Assert.Empty(repository.GetAll());
            Assert.Null(repository.GetBySku("3001"));
        }
    }
}
