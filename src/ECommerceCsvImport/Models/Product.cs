namespace ECommerceCsvImport.Models
{
    // Товар магазина аксессуаров сервисного центра. Вариант 3 (поля размер, цвет, материал)
    public class Product
    {
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "RUB";
        public string CategoryName { get; set; } = "Без категории";
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        // Поля варианта 3
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
    }
}
