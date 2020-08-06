using System;
using System.Globalization;
using AutoMapper;
using Avanti.Core.Microservice;
using Avanti.Core.Microservice.Actors;
using Avanti.Core.Unittests;
using Avanti.OrderWarehouseService.Order;
using Avanti.OrderWarehouseService.Order.Events;
using Avanti.OrderWarehouseService.Order.Mappings;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Avanti.OrderWarehouseServiceTests.Order
{
    public class EventProcessorSpec : WithSubject<EventProcessor>
    {
        private ProgrammableActor<ProcessingCoordinatorActor> progProcessingCoordinatorActor;

        public EventProcessorSpec()
        {
            this.progProcessingCoordinatorActor = Kit.CreateProgrammableActor<ProcessingCoordinatorActor>("processing-coordinator-actor");
            var processingCoordinatorActorProvider = An<IActorProvider<ProcessingCoordinatorActor>>();
            processingCoordinatorActorProvider.Get().Returns(this.progProcessingCoordinatorActor.TestProbe);

            var config = new MapperConfiguration(cfg => cfg.AddProfile(new OrderMapping()));
            config.AssertConfigurationIsValid();

            Subject = new EventProcessor(processingCoordinatorActorProvider, config.CreateMapper());
        }

        [Fact]
        public async void Should_Return_Success_When_Processed()
        {
            this.progProcessingCoordinatorActor.SetResponseForRequest<ProcessingCoordinatorActor.ProcessOrder>(
                r => new ProcessingCoordinatorActor.OrderIsProcessed());

            var result = await Subject.ProcessEvent(
                insertedEvent,
                DateTimeOffset.Parse("2020-12-31T07:00:00Z", CultureInfo.InvariantCulture));

            result.Should().BeOfType<Success>();

            this.progProcessingCoordinatorActor.GetRequest<ProcessingCoordinatorActor.ProcessOrder>().Should().BeEquivalentTo(
                new ProcessingCoordinatorActor.ProcessOrder
                {
                    OrderId = 5,
                    OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture)
                });
        }

        [Fact]
        public async void Should_Return_Success_When_Duplicate_Order()
        {
            this.progProcessingCoordinatorActor.SetResponseForRequest<ProcessingCoordinatorActor.ProcessOrder>(
                r => new ProcessingCoordinatorActor.OrderIsDuplicate());

            var result = await Subject.ProcessEvent(
                insertedEvent,
                DateTimeOffset.Parse("2020-12-31T07:00:00Z", CultureInfo.InvariantCulture));

            result.Should().BeOfType<Success>();
        }

        [Fact]
        public async void Should_Return_Success_When_Partially_Processed()
        {
            this.progProcessingCoordinatorActor.SetResponseForRequest<ProcessingCoordinatorActor.ProcessOrder>(
                r => new ProcessingCoordinatorActor.OrderIsPartiallyProcessed());

            var result = await Subject.ProcessEvent(
                insertedEvent,
                DateTimeOffset.Parse("2020-12-31T07:00:00Z", CultureInfo.InvariantCulture));

            result.Should().BeOfType<Success>();
        }

        [Fact]
        public async void Should_Return_Failure_When_Failed_To_Process()
        {
            this.progProcessingCoordinatorActor.SetResponseForRequest<ProcessingCoordinatorActor.ProcessOrder>(
                r => new ProcessingCoordinatorActor.OrderFailedToProcess());

            var result = await Subject.ProcessEvent(
                insertedEvent,
                DateTimeOffset.Parse("2020-12-31T07:00:00Z", CultureInfo.InvariantCulture));

            result.Should().BeOfType<Failure>();
        }

        private OrderInserted insertedEvent = new OrderInserted
        {
            Id = 5,
            OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture)
        };
    }
}
