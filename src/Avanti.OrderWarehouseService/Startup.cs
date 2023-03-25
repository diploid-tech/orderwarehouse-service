using System.CodeDom.Compiler;
using Microsoft.AspNetCore.Authorization;

namespace Avanti.OrderWarehouseService;

[GeneratedCode("avanti-cli", "1.7.0-beta+35a2c2c9e6")]
public class ExtendedConfiguration : IExtendedConfiguration
{
    public void AdditionalAuthorizationSetup(AuthorizationOptions options)
    { }

    public void AdditionalConfigure(WebApplication app)
    { }

    public void AdditionalServices(IServiceCollection services)
    { }
}
