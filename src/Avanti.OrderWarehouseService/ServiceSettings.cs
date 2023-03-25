using Avanti.Core.Microservice.Settings;

namespace Avanti.OrderWarehouseService;

public class ServiceSettings : IValidatable
{
    public Uri? OrderServiceUri { get; set; }
    public Uri? ProductServiceUri { get; set; }
    public IDictionary<string, Uri> WarehouseServiceUris { get; } = new Dictionary<string, Uri>();
}
