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

namespace Avanti.OrderWarehouseServiceTests.WarehouseOrder
{
    public partial class ProcessingUnitActorSpec : WithSubject<IActorRef>
    {
        private ProgrammableActor<HttpRequestActor> progHttpRequestActor;
        private ProgrammableActor<WarehouseSendActor> progWarehouseSendActor;
        private ServiceSettings settings = new ServiceSettings
        {
            OrderServiceUri = new Uri("http://order-service:5000"),
            ProductServiceUri = new Uri("http://product-service:5000")
        };

        private ProcessingUnitActorSpec()
        {
            this.progHttpRequestActor = Kit.CreateProgrammableActor<HttpRequestActor>("http-request-actor");
            var httpRequestActorProvider = An<IActorProvider<HttpRequestActor>>();
            httpRequestActorProvider.Get().Returns(this.progHttpRequestActor.TestProbe);

            this.progWarehouseSendActor = Kit.CreateProgrammableActor<WarehouseSendActor>("warehouse-send-actor");

            var options = An<IOptions<ServiceSettings>>();
            options.Value.Returns(this.settings);

            var config = new MapperConfiguration(cfg => cfg.AddProfile(new WarehouseOrderMapping()));
            config.AssertConfigurationIsValid();

            Subject = Sys.ActorOf(
                Props.Create<ActorUnderTest>(
                    httpRequestActorProvider,
                    options,
                    config.CreateMapper(),
                    this.progWarehouseSendActor.TestProbe));
        }

        public class ActorUnderTest : ProcessingUnitActor
        {
            private TestProbe warehouseTestProbe;

            public ActorUnderTest(
                IActorProvider<HttpRequestActor> httpRequestActorProvider,
                IOptions<ServiceSettings> serviceSettings,
                IMapper mapper,
                TestProbe warehouseTestProbe)
                    : base(httpRequestActorProvider, serviceSettings, mapper)
            {
                this.warehouseTestProbe = warehouseTestProbe;
            }

            protected override IActorRef CreateWarehouseSendActorRef(string actorName) => this.warehouseTestProbe;
        }
    }
}
