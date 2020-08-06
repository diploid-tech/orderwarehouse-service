using System;
using System.Collections.Generic;

namespace Avanti.OrderWarehouseService.WarehouseOrder.Models
{
    public class Order
    {
        public int Id { get; set; }

        public DateTimeOffset OrderDate { get; set; }
        public IEnumerable<OrderLine> Lines { get; set; } = Array.Empty<OrderLine>();

        public class OrderLine
        {
            public int Line { get; set; }
            public int ProductId { get; set; }
            public Product? Product { get; set; }
            public int Amount { get; set; }
        }
    }
}
