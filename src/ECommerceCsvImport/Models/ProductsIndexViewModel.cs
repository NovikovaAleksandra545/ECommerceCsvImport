namespace ECommerceCsvImport.Models
{
    public class ProductsIndexViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();
        public int AllCount { get; set; }

        // Параметры поиска и фильтров
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? Size { get; set; }
        public string? Sort { get; set; }

        public List<string> Categories { get; set; } = new List<string>();
        public List<string> Sizes { get; set; } = new List<string>();

        public int TotalStock => Products.Sum(p => p.StockQuantity);
        public decimal TotalValue => Products.Sum(p => p.Price * p.StockQuantity);
    }
}
