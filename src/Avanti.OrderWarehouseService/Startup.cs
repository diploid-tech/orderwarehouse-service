using System.CodeDom.Compiler;
using Avanti.Core.Microservice;
using Microsoft.Extensions.Configuration;

namespace Avanti.OrderWarehouseService
{
    [GeneratedCode("avanti-cli", "2020-R1")]
    public class Startup : StartupCore<ServiceSettings>
    {
        public Startup(IConfiguration config)
            : base(config)
        { }
    }
}
