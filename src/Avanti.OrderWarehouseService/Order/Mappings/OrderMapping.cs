using AutoMapper;
using Avanti.OrderWarehouseService.Order.Events;
using Avanti.OrderWarehouseService.WarehouseOrder;

namespace Avanti.OrderWarehouseService.Order.Mappings
{
    public class OrderMapping : Profile
    {
        public OrderMapping()
        {
            CreateMap<OrderInserted, ProcessingCoordinatorActor.ProcessOrder>()
                .ForMember(s => s.OrderId, o => o.MapFrom(s => s.Id));
            CreateMap<ProcessingCoordinatorActor.ProcessOrder, ProcessingUnitActor.StartProcessing>();
        }
    }
}
