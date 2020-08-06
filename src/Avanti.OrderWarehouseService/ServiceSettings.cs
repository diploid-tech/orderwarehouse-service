using System;
using System.Collections.Generic;
using Avanti.Core.Microservice.Settings;

namespace Avanti.OrderWarehouseService
{
    public class ServiceSettings : Validatable
    {
        public Uri? OrderServiceUri { get; set; }
        public Uri? ProductServiceUri { get; set; }
        public IDictionary<string, Uri> WarehouseServiceUris { get; } = new Dictionary<string, Uri>();
    }
}
