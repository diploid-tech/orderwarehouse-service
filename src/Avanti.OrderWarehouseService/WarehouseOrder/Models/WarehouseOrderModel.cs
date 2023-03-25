namespace Avanti.OrderWarehouseService.WarehouseOrder.Models;

public class WarehouseOrderModel
{
    public string? Id { get; set; }
    public int OrderId { get; set; }
    public int WarehouseId { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public IEnumerable<OrderLine> Lines { get; set; } = Array.Empty<OrderLine>();

    public class OrderLine
    {
        public int Line { get; set; }
        public int ProductId { get; set; }
        public string Description { get; set; } = "unknown";
        public int Amount { get; set; }
    }
}
