using System.CodeDom.Compiler;
using Avanti.Core.Microservice;

namespace Avanti.OrderWarehouseService
{
    [GeneratedCode("avanti-cli", "2020-R1")]
    public static class Program
    {
        public static void Main()
        {
            Service.Run<Startup>();
        }
    }
}
