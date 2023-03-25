using System;
using System.Globalization;
using Akka.Actor;
using Akka.TestKit;
using Avanti.OrderWarehouseService.Order;
using Avanti.OrderWarehouseService.WarehouseOrder;
using FluentAssertions;
using Xunit;

namespace Avanti.OrderWarehouseServiceTests.Order;

public partial class ProcessingCoordinatorActorSpec
{
    public class When_Process_Order : ProcessingCoordinatorActorSpec
    {
        [Fact]
        public void Should_Return_Order_Is_Processed_When_Processing_Unit_Actor_Returns_Processed()
        {
            progProcessingUnitActor.SetResponseForRequest<ProcessingUnitActor.StartProcessing>(
                r => new ProcessingCoordinatorActor.OrderIsProcessed());

            Subject.Tell(
                new ProcessingCoordinatorActor.ProcessOrder
                {
                    OrderId = 5,
                    OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture)
                });

            Kit.ExpectMsg<ProcessingCoordinatorActor.OrderIsProcessed>();

            progProcessingUnitActor.GetRequest<ProcessingUnitActor.StartProcessing>().Should().BeEquivalentTo(
                new ProcessingUnitActor.StartProcessing
                {
                    OrderId = 5,
                    OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture)
                });
        }

        [Fact]
        public void Should_Return_Order_Not_Processed_When_Processing_Unit_Actor_Returns_Not_Processed()
        {
            progProcessingUnitActor.SetResponseForRequest<ProcessingUnitActor.StartProcessing>(
                r => new ProcessingCoordinatorActor.OrderFailedToProcess());

            Subject.Tell(
                new ProcessingCoordinatorActor.ProcessOrder
                {
                    OrderId = 5,
                    OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture)
                });

            Kit.ExpectMsg<ProcessingCoordinatorActor.OrderFailedToProcess>();
        }

        [Fact]
        public void Should_Return_Order_Partially_Processed_When_Processing_Unit_Actor_Returns_Partially_Processed()
        {
            progProcessingUnitActor.SetResponseForRequest<ProcessingUnitActor.StartProcessing>(
                r => new ProcessingCoordinatorActor.OrderIsPartiallyProcessed());

            Subject.Tell(
                new ProcessingCoordinatorActor.ProcessOrder
                {
                    OrderId = 5,
                    OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture)
                });

            Kit.ExpectMsg<ProcessingCoordinatorActor.OrderIsPartiallyProcessed>();
        }

        [Fact]
        public void Should_Return_Order_Duplicate_When_Has_Existing_Order()
        {
            Subject.As<TestActorRef<ActorUnderTest>>().UnderlyingActor.HasExistingOrder = true;

            Subject.Tell(
                new ProcessingCoordinatorActor.ProcessOrder
                {
                    OrderId = 5,
                    OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture)
                });

            Kit.ExpectMsg<ProcessingCoordinatorActor.OrderIsDuplicate>();
        }
    }
}
