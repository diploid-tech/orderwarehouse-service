using System;
using Akka.Actor;
using Akka.TestKit;
using AutoMapper;
using Avanti.Core.Http;
using Avanti.Core.Microservice.Actors;
using Avanti.Core.Unittests;
using Avanti.OrderWarehouseService;
using Avanti.OrderWarehouseService.WarehouseOrder;
using Avanti.OrderWarehouseService.WarehouseOrder.Mappings;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Avanti.OrderWarehouseServiceTests.WarehouseOrder;

public partial class ProcessingUnitActorSpec : WithSubject<IActorRef>
{
    private readonly ProgrammableActor<HttpRequestActor> progHttpRequestActor;
    private readonly ProgrammableActor<WarehouseSendActor> progWarehouseSendActor;
    private readonly ServiceSettings settings = new()
    {
        OrderServiceUri = new Uri("http://order-service:5000"),
        ProductServiceUri = new Uri("http://product-service:5000")
    };

    private ProcessingUnitActorSpec()
    {
        progHttpRequestActor = Kit.CreateProgrammableActor<HttpRequestActor>("http-request-actor");
        IActorProvider<HttpRequestActor> httpRequestActorProvider = An<IActorProvider<HttpRequestActor>>();
        httpRequestActorProvider.Get().Returns(progHttpRequestActor.TestProbe);

        progWarehouseSendActor = Kit.CreateProgrammableActor<WarehouseSendActor>("warehouse-send-actor");

        IOptions<ServiceSettings> options = An<IOptions<ServiceSettings>>();
        options.Value.Returns(settings);

        var config = new MapperConfiguration(cfg => cfg.AddProfile(new WarehouseOrderMapping()));
        config.AssertConfigurationIsValid();

        Subject = Sys.ActorOf(
            Props.Create<ActorUnderTest>(
                httpRequestActorProvider,
                options,
                config.CreateMapper(),
                progWarehouseSendActor.TestProbe));
    }

    public class ActorUnderTest : ProcessingUnitActor
    {
        private readonly TestProbe warehouseTestProbe;

        public ActorUnderTest(
            IActorProvider<HttpRequestActor> httpRequestActorProvider,
            IOptions<ServiceSettings> serviceSettings,
            IMapper mapper,
            TestProbe warehouseTestProbe)
                : base(httpRequestActorProvider, serviceSettings, mapper)
        {
            this.warehouseTestProbe = warehouseTestProbe;
        }

        protected override IActorRef CreateWarehouseSendActorRef(string actorName) => warehouseTestProbe;
    }
}
