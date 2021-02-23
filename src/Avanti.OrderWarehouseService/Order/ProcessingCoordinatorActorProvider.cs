using Akka.Actor;
using Avanti.Core.Microservice.Actors;
using Avanti.Core.Microservice.AkkaSupport;

namespace Avanti.OrderWarehouseService.Order
{
    public class ProcessingCoordinatorActorProvider : BaseActorProvider<ProcessingCoordinatorActor>
    {
        public ProcessingCoordinatorActorProvider(ActorSystem actorSystem)
        {
            this.ActorRef = actorSystem.ActorOfWithDI<ProcessingCoordinatorActor>("processing-coordinator-actor");
        }
    }
}
