using System;
using Akka.Actor;
using Akka.TestKit;
using Avanti.Core.Http;
using Avanti.Core.Microservice.Actors;
using Avanti.Core.Unittests;
using Avanti.OrderWarehouseService;
using Avanti.OrderWarehouseService.WarehouseOrder;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Avanti.OrderWarehouseServiceTests.WarehouseOrder
{
    public partial class WarehouseSendActorSpec : WithSubject<IActorRef>
    {
        private ProgrammableActor<HttpRequestActor> progHttpRequestActor;
        private TestProbe parentTestProbe;
        private ServiceSettings settings = new ServiceSettings
        {
            WarehouseServiceUris = {
                { "1", new Uri("http://warehouse-one-service:5000") },
                { "2", new Uri("http://warehouse-two-service:5000") }
            }
        };

        private WarehouseSendActorSpec()
        {
            this.progHttpRequestActor = Kit.CreateProgrammableActor<HttpRequestActor>("http-request-actor");
            var httpRequestActorProvider = An<IActorProvider<HttpRequestActor>>();
            httpRequestActorProvider.Get().Returns(this.progHttpRequestActor.TestProbe);

            var options = An<IOptions<ServiceSettings>>();
            options.Value.Returns(this.settings);

            parentTestProbe = Kit.CreateTestProbe("parent-actor");

            Subject = Sys.ActorOf(
                Props.Create<ActorUnderTest>(
                    httpRequestActorProvider,
                    options,
                    parentTestProbe));
        }

        public class ActorUnderTest : WarehouseSendActor
        {
            private TestProbe parentTestProbe;

            public ActorUnderTest(
                IActorProvider<HttpRequestActor> httpRequestActorProvider,
                IOptions<ServiceSettings> serviceSettings,
                TestProbe parentTestProbe)
                    : base(httpRequestActorProvider, serviceSettings)
            {
                this.parentTestProbe = parentTestProbe;
            }

            protected override IActorRef Parent { get => this.parentTestProbe; }
        }
    }
}
