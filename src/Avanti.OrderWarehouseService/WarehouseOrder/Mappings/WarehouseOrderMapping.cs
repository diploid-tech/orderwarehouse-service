using System.Linq;
using AutoMapper;
using Avanti.OrderWarehouseService.Order.Events;

namespace Avanti.OrderWarehouseService.WarehouseOrder.Mappings
{
    public class WarehouseOrderMapping : Profile
    {
        public WarehouseOrderMapping()
        {
            CreateMap<(Models.Order Order, IGrouping<int, Models.Order.OrderLine> Lines), Models.WarehouseOrder>()
                .ForMember(s => s.WarehouseId, o => o.MapFrom(s => s.Lines.Key))
                .ForMember(s => s.Id, o => o.MapFrom(s => $"{s.Order.Id}-{s.Lines.Key}"))
                .ForMember(s => s.Lines, o => o.MapFrom(s => s.Lines.AsEnumerable()))
                .AfterMap((src, dest, context) => context.Mapper.Map(src.Order, dest))
                .ForAllOtherMembers(o => o.Ignore());

            CreateMap<Models.Order, Models.WarehouseOrder>()
                .ForMember(s => s.WarehouseId, o => o.Ignore())
                .ForMember(s => s.Lines, o => o.Ignore())
                .ForMember(s => s.Id, o => o.Ignore())
                .ForMember(s => s.OrderId, o => o.MapFrom(s => s.Id));
            CreateMap<Models.Order.OrderLine, Models.WarehouseOrder.OrderLine>()
                .ForMember(s => s.Description, o => o.MapFrom(s => s.Product == null ? "Unknown" : s.Product.Description));
        }
    }
}
