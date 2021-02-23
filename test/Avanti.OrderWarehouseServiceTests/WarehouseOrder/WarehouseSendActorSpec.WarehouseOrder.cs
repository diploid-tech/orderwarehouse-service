using System;
using System.Globalization;
using System.Net;
using Akka.Actor;
using Avanti.Core.Http;
using Avanti.OrderWarehouseService.WarehouseOrder;
using FluentAssertions;
using Xunit;
using Models = Avanti.OrderWarehouseService.WarehouseOrder.Models;

namespace Avanti.OrderWarehouseServiceTests.WarehouseOrder
{
    public partial class WarehouseSendActorSpec
    {
        public class When_Receiving_WarehouseOrder : WarehouseSendActorSpec
        {
            private readonly Models.WarehouseOrderModel order = new()
            {
                Id = "5-1",
                OrderId = 5,
                WarehouseId = 2,
                OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture),
                Lines = new[]
                {
                    new Models.WarehouseOrderModel.OrderLine { Line = 1, ProductId = 5, Description = "product 1", Amount = 5 }
                }
            };

            [Fact]
            public void Should_Return_Order_Is_Sent_When_Warehouse_Service_Received_Order()
            {
                // Arrange
                progHttpRequestActor.SetResponseForRequest<HttpRequestActor.Post>(_ =>
                    new HttpRequestActor.ReceivedSuccessServiceResponse(HttpStatusCode.OK, Array.Empty<byte>()));

                // Act
                Subject.Tell(order);

                // Assert
                parentTestProbe.ExpectMsg<WarehouseSendActor.OrderIsSent>().Should().BeEquivalentTo(new WarehouseSendActor.OrderIsSent { WarehouseId = 2 });
            }

            [Fact]
            public void Should_Return_Order_Is_Not_Sent_When_Warehouse_Service_Returned_Non_OK()
            {
                // Arrange
                progHttpRequestActor.SetResponseForRequest<HttpRequestActor.Post>(_ =>
                    new HttpRequestActor.ReceivedNonSuccessServiceResponse(HttpStatusCode.BadGateway, Array.Empty<byte>()));

                // Act
                Subject.Tell(order);

                // Assert
                parentTestProbe.ExpectMsg<WarehouseSendActor.OrderIsNotSent>().Should().BeEquivalentTo(new WarehouseSendActor.OrderIsNotSent { WarehouseId = 2 });
            }

            [Fact]
            public void Should_Return_Order_Is_Not_Sent_When_Warehouse_Service_Communication_Failure()
            {
                // Arrange
                progHttpRequestActor.SetResponseForRequest<HttpRequestActor.Post>(_ =>
                    new HttpRequestActor.ServiceRequestFailed(HttpRequestActor.ServiceRequestFailed.FailureReason.ServiceRequestTimeout));

                // Act
                Subject.Tell(order);

                // Assert
                parentTestProbe.ExpectMsg<WarehouseSendActor.OrderIsNotSent>().Should().BeEquivalentTo(new WarehouseSendActor.OrderIsNotSent { WarehouseId = 2 });
            }

            [Fact]
            public void Should_Return_Order_Is_Not_Sent_When_Warehouse_Service_Setting_Cannot_Be_Found()
            {
                // Arrange
                settings.WarehouseServiceUris.Clear();

                // Act
                Subject.Tell(order);

                // Assert
                parentTestProbe.ExpectMsg<WarehouseSendActor.OrderIsNotSent>().Should().BeEquivalentTo(new WarehouseSendActor.OrderIsNotSent { WarehouseId = 2 });
            }
        }
    }
}
