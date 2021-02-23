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
        private readonly ProgrammableActor<HttpRequestActor> progHttpRequestActor;
        private readonly TestProbe parentTestProbe;
        private readonly ServiceSettings settings = new()
        {
            WarehouseServiceUris =
            {
                { "1", new Uri("http://warehouse-one-service:5000") },
                { "2", new Uri("http://warehouse-two-service:5000") }
            }
        };

        private WarehouseSendActorSpec()
        {
            progHttpRequestActor = Kit.CreateProgrammableActor<HttpRequestActor>("http-request-actor");
            IActorProvider<HttpRequestActor> httpRequestActorProvider = An<IActorProvider<HttpRequestActor>>();
            httpRequestActorProvider.Get().Returns(progHttpRequestActor.TestProbe);

            IOptions<ServiceSettings> options = An<IOptions<ServiceSettings>>();
            options.Value.Returns(settings);

            parentTestProbe = Kit.CreateTestProbe("parent-actor");

            Subject = Sys.ActorOf(
                Props.Create<ActorUnderTest>(
                    httpRequestActorProvider,
                    options,
                    parentTestProbe));
        }

        public class ActorUnderTest : WarehouseSendActor
        {
            private readonly TestProbe parentTestProbe;

            public ActorUnderTest(
                IActorProvider<HttpRequestActor> httpRequestActorProvider,
                IOptions<ServiceSettings> serviceSettings,
                TestProbe parentTestProbe)
                    : base(httpRequestActorProvider, serviceSettings)
            {
                this.parentTestProbe = parentTestProbe;
            }

            protected override IActorRef Parent => parentTestProbe;
        }
    }
}
