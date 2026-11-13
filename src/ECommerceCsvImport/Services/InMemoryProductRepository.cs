using ECommerceCsvImport.Models;

namespace ECommerceCsvImport.Services
{
    /// <summary>
    /// Хранение товаров в памяти (List). Репозиторий зарегистрирован как singleton,
    /// поэтому доступ к списку закрыт через lock - запросы могут идти одновременно.
    /// </summary>
    public class InMemoryProductRepository : IProductRepository
    {
        private readonly List<Product> _products = new List<Product>();
        private readonly object _lock = new object();

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _products.Count;
                }
            }
        }

        public void Add(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            lock (_lock)
            {
                _products.Add(product);
            }
        }

        public IReadOnlyList<Product> GetAll()
        {
            lock (_lock)
            {
                // возвращаем копию, чтобы список нельзя было изменить снаружи
                return _products.ToList();
            }
        }

        // Поиск по артикулу без учета регистра и пробелов
        public Product? GetBySku(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return null;

            lock (_lock)
            {
                return _products.FirstOrDefault(p =>
                    string.Equals(p.Sku, sku.Trim(), StringComparison.OrdinalIgnoreCase));
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _products.Clear();
            }
        }
    }
}
