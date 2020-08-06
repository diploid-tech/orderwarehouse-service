using System;
using System.Collections.Generic;
using Akka.Actor;

namespace Avanti.OrderWarehouseService.Order
{
    public partial class ProcessingCoordinatorActor
    {
        public class ProcessOrder
        {
            public int OrderId { get; set; }
            public DateTimeOffset OrderDate { get; set; }
        }
    }
}
