using Akka.Actor;

namespace Avanti.OrderWarehouseService.WarehouseOrder
{
    public partial class WarehouseSendActor
    {
        public interface IResponse { }

        public class OrderIsSent : IResponse
        {
            public int WarehouseId { get; set; }
        }

        public class OrderIsNotSent : IResponse
        {
            public int WarehouseId { get; set; }
        }
    }
}
