using System;
using Akka.Actor;
using Akka.TestKit;
using AutoMapper;
using Avanti.Core.Unittests;
using Avanti.OrderWarehouseService.Order;
using Avanti.OrderWarehouseService.Order.Mappings;
using Avanti.OrderWarehouseService.WarehouseOrder;

namespace Avanti.OrderWarehouseServiceTests.Order
{
    public partial class ProcessingCoordinatorActorSpec : WithSubject<IActorRef>
    {
        private ProgrammableActor<ProcessingUnitActor> progProcessingUnitActor;
        private ProcessingCoordinatorActorSpec()
        {
            this.progProcessingUnitActor = Kit.CreateProgrammableActor<ProcessingUnitActor>("processing-unit-actor");

            var config = new MapperConfiguration(cfg => cfg.AddProfile(new OrderMapping()));
            config.AssertConfigurationIsValid();

            Subject = this.ActorOfAsTestActorRef<ActorUnderTest>(
                Props.Create<ActorUnderTest>(config.CreateMapper(), this.progProcessingUnitActor.TestProbe));
        }

        public class ActorUnderTest : ProcessingCoordinatorActor
        {
            private TestProbe processingUnitTestProbe;
            public bool HasExistingOrder { get; set; }

            public ActorUnderTest(IMapper mapper, TestProbe processingUnitTestProbe)
                : base(mapper)
            {
                this.processingUnitTestProbe = processingUnitTestProbe;
            }

            protected override bool ChildExists(string actorName) => this.HasExistingOrder;

            protected override IActorRef CreateProcessingUnitActor(string actorName) => this.processingUnitTestProbe;

            protected override void PreStart() { }
        }
    }
}
