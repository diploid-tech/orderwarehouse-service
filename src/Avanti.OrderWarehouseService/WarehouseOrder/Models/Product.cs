namespace Avanti.OrderWarehouseService.WarehouseOrder.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Description { get; set; } = "Unknown";
        public int Price { get; set; }
        public int WarehouseId { get; set; }
    }
}
