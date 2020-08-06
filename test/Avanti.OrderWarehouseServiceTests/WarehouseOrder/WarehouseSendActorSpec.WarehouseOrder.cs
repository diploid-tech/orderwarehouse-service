using System;
using System.Globalization;
using System.Net;
using Akka.Actor;
using Akka.TestKit;
using Avanti.Core.Http;
using Avanti.Core.Microservice.Actors;
using Avanti.Core.Unittests;
using Avanti.OrderWarehouseService;
using Avanti.OrderWarehouseService.WarehouseOrder;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using Models = Avanti.OrderWarehouseService.WarehouseOrder.Models;

namespace Avanti.OrderWarehouseServiceTests.WarehouseOrder
{
    public partial class WarehouseSendActorSpec
    {
        public class When_Receiving_WarehouseOrder : WarehouseSendActorSpec
        {
            [Fact]
            public void Should_Return_Order_Is_Sent_When_Warehouse_Service_Received_Order()
            {
                // Arrange
                this.progHttpRequestActor.SetResponseForRequest<HttpRequestActor.Post>(_ =>
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
                this.progHttpRequestActor.SetResponseForRequest<HttpRequestActor.Post>(_ =>
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
                this.progHttpRequestActor.SetResponseForRequest<HttpRequestActor.Post>(_ =>
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
                this.settings.WarehouseServiceUris.Clear();

                // Act
                Subject.Tell(order);

                // Assert
                parentTestProbe.ExpectMsg<WarehouseSendActor.OrderIsNotSent>().Should().BeEquivalentTo(new WarehouseSendActor.OrderIsNotSent { WarehouseId = 2 });
            }

            private Models.WarehouseOrder order = new Models.WarehouseOrder
            {
                Id = "5-1",
                OrderId = 5,
                WarehouseId = 2,
                OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture),
                Lines = new[]
                {
                    new Models.WarehouseOrder.OrderLine { Line = 1, ProductId = 5, Description = "product 1", Amount = 5 }
                }
            };
        }
    }
}
