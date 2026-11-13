namespace ECommerceCsvImport.Models
{
    // Товар магазина. Вариант 3 - "Одежда" (добавлены размер, цвет и материал)
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

        // Поля варианта "Одежда"
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
    }
}
