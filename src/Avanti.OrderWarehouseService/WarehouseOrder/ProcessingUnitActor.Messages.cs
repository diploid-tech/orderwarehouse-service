using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;

namespace Avanti.OrderWarehouseService.WarehouseOrder
{
    public partial class ProcessingUnitActor
    {
        public class StartProcessing
        {
            public int OrderId { get; set; }
            public DateTimeOffset OrderDate { get; set; }
        }
    }
}
