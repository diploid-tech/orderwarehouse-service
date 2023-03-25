using Akka.Actor;
using Akka.TestKit;
using AutoMapper;
using Avanti.Core.Unittests;
using Avanti.OrderWarehouseService.Order;
using Avanti.OrderWarehouseService.Order.Mappings;
using Avanti.OrderWarehouseService.WarehouseOrder;

namespace Avanti.OrderWarehouseServiceTests.Order;

public partial class ProcessingCoordinatorActorSpec : WithSubject<IActorRef>
{
    private readonly ProgrammableActor<ProcessingUnitActor> progProcessingUnitActor;

    private ProcessingCoordinatorActorSpec()
    {
        progProcessingUnitActor = Kit.CreateProgrammableActor<ProcessingUnitActor>("processing-unit-actor");

        var config = new MapperConfiguration(cfg => cfg.AddProfile(new OrderMapping()));
        config.AssertConfigurationIsValid();

        Subject = ActorOfAsTestActorRef<ActorUnderTest>(
            Props.Create<ActorUnderTest>(config.CreateMapper(), progProcessingUnitActor.TestProbe));
    }

    public class ActorUnderTest : ProcessingCoordinatorActor
    {
        private readonly TestProbe processingUnitTestProbe;

        public ActorUnderTest(IMapper mapper, TestProbe processingUnitTestProbe)
            : base(mapper)
        {
            this.processingUnitTestProbe = processingUnitTestProbe;
        }

        public bool HasExistingOrder { get; set; }

        protected override bool ChildExists(string actorName) => HasExistingOrder;

        protected override IActorRef CreateProcessingUnitActor(string actorName) => processingUnitTestProbe;

        protected override void PreStart()
        {
        }
    }
}
