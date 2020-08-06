using System;
using System.Threading.Tasks;
using Akka.Actor;
using AutoMapper;
using Avanti.Core.EventStream.Processor;
using Avanti.Core.Microservice;
using Avanti.Core.Microservice.Actors;
using Avanti.OrderWarehouseService.Order.Events;
using Microsoft.Extensions.Logging;

namespace Avanti.OrderWarehouseService.Order
{
    public class EventProcessor : EventProcessor<OrderInserted>
    {
        private readonly IMapper mapper;
        private readonly IActorRef processingCoordinatorActor;

        public EventProcessor(
            IActorProvider<ProcessingCoordinatorActor> processingCoordinatorActorProvider,
            IMapper mapper)
        {
            this.processingCoordinatorActor = processingCoordinatorActorProvider.Get();
            this.mapper = mapper;
        }

        public async override Task<Result> ProcessEvent(OrderInserted e, DateTimeOffset eventTimeStamp) =>
            await this.processingCoordinatorActor.Ask(this.mapper.Map<ProcessingCoordinatorActor.ProcessOrder>(e)) switch
            {
                ProcessingCoordinatorActor.OrderIsDuplicate _ => new Success(),
                ProcessingCoordinatorActor.OrderIsProcessed _ => new Success(),
                ProcessingCoordinatorActor.OrderIsPartiallyProcessed _ => new Success(),
                ProcessingCoordinatorActor.OrderFailedToProcess f => $"Failed to process order {e.Id}".Failure(),
                _ => $"Unknown error while processing order {e.Id}".Failure()
            };
    }
}
