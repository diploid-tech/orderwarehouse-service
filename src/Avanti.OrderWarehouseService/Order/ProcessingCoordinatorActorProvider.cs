using System.Diagnostics.CodeAnalysis;
using Akka.Actor;
using Akka.DI.Core;
using Avanti.Core.Microservice.Actors;

namespace Avanti.OrderWarehouseService.Order
{
    public class ProcessingCoordinatorActorProvider : BaseActorProvider<ProcessingCoordinatorActor>
    {
        public ProcessingCoordinatorActorProvider(ActorSystem actorRefFactory) =>
            this.ActorRef = actorRefFactory.ActorOf(actorRefFactory.DI().Props<ProcessingCoordinatorActor>(), "processing-coordinator-actor");
    }
}
