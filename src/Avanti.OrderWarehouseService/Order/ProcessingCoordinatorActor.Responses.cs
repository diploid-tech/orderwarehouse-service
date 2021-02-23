namespace Avanti.OrderWarehouseService.Order
{
    public partial class ProcessingCoordinatorActor
    {
        public interface IResponse { }

        public class OrderIsDuplicate : IResponse { }

        public class OrderFailedToProcess : IResponse { }

        public class OrderIsProcessed : IResponse { }

        public class OrderIsPartiallyProcessed : IResponse { }
    }
}
