using AutoMapper;

namespace Avanti.OrderWarehouseService.WarehouseOrder.Mappings;

public class WarehouseOrderMapping : Profile
{
    public WarehouseOrderMapping()
    {
        CreateMap<(Models.OrderModel Order, IGrouping<int, Models.OrderModel.OrderLine> Lines), Models.WarehouseOrderModel>()
            .ForMember(s => s.WarehouseId, o => o.MapFrom(s => s.Lines.Key))
            .ForMember(s => s.Id, o => o.MapFrom(s => $"{s.Order.Id}-{s.Lines.Key}"))
            .ForMember(s => s.Lines, o => o.MapFrom(s => s.Lines.AsEnumerable()))
            .ForMember(s => s.OrderId, o => o.Ignore())
            .ForMember(s => s.OrderDate, o => o.Ignore())
            .AfterMap((src, dest, context) => context.Mapper.Map(src.Order, dest));

        CreateMap<Models.OrderModel, Models.WarehouseOrderModel>()
            .ForMember(s => s.WarehouseId, o => o.Ignore())
            .ForMember(s => s.Lines, o => o.Ignore())
            .ForMember(s => s.Id, o => o.Ignore())
            .ForMember(s => s.OrderId, o => o.MapFrom(s => s.Id));
        CreateMap<Models.OrderModel.OrderLine, Models.WarehouseOrderModel.OrderLine>()
            .ForMember(s => s.Description, o => o.MapFrom(s => s.Product == null ? "Unknown" : s.Product.Description));
    }
}
