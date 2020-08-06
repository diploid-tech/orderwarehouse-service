using System.Text.Json.Serialization;
using Akka.Actor;
using Akka.DI.Core;
using Akka.Event;
using Akka.Logger.Serilog;
using AutoMapper;
using Avanti.Core.EventStream;
using Avanti.OrderWarehouseService.WarehouseOrder;

namespace Avanti.OrderWarehouseService.Order
{
    public partial class ProcessingCoordinatorActor : ReceiveActor
    {
        private readonly ILoggingAdapter logger = Context.GetLogger<SerilogLoggingAdapter>();
        private readonly IMapper mapper;

        public ProcessingCoordinatorActor(IMapper mapper)
        {
            this.mapper = mapper;

            Receive<ProcessOrder>(Handle);
        }

        private void Handle(ProcessOrder m)
        {
            var actorName = $"order-{m.OrderId}";
            if (!ChildExists(actorName))
            {
                var actor = CreateProcessingUnitActor(actorName);
                actor.Forward(this.mapper.Map<ProcessingUnitActor.StartProcessing>(m));
            }
            else
            {
                this.logger.Warning($"Already processing this order with id {m.OrderId}");
                Sender.Tell(new OrderIsDuplicate());
            }
        }

        protected virtual bool ChildExists(string actorName) =>
            !Context.Child(actorName).IsNobody();

        protected virtual IActorRef CreateProcessingUnitActor(string actorName) =>
            Context.ActorOf(Context.DI().Props<ProcessingUnitActor>(), actorName);

        protected override void PreStart()
        {
            Context.CreateEventWorkerActor<Events.OrderInserted>("ordercreated-event-processing-actor");
        }
    }
}
