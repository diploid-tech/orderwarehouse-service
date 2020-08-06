using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Akka.Actor;
using Avanti.Core.Http;
using Avanti.OrderWarehouseService.Order;
using Avanti.OrderWarehouseService.WarehouseOrder;
using FluentAssertions;
using Xunit;
using Models = Avanti.OrderWarehouseService.WarehouseOrder.Models;

namespace Avanti.OrderWarehouseServiceTests.WarehouseOrder
{
    public partial class ProcessingUnitActorSpec
    {
        public class When_Start_Processing : ProcessingUnitActorSpec
        {
            public When_Start_Processing()
            {
                this.progHttpRequestActor.SetResponseForRequest<HttpRequestActor.Get>(_ =>
                    new HttpRequestActor.ReceivedSuccessServiceResponse(
                        HttpStatusCode.OK,
                        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Models.Order
                        {
                            Id = 5,
                            OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture),
                            Lines = new[]
                            {
                                new Models.Order.OrderLine { Line = 1, ProductId = 5, Amount = 5 },
                                new Models.Order.OrderLine { Line = 2, ProductId = 10, Amount = 2 }
                            }
                        }))));

                this.progHttpRequestActor.SetResponseForRequest<HttpRequestActor.Post>(_ =>
                    new HttpRequestActor.ReceivedSuccessServiceResponse(
                        HttpStatusCode.OK,
                        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new[]
                        {
                            new Models.Product { Id = 5, Description = "product 1", Price = 5000, WarehouseId = 1 },
                            new Models.Product { Id = 10, Description = "product 2", Price = 7500, WarehouseId = 2 },
                        }))));

                this.progWarehouseSendActor.SetResponseForRequest<Models.WarehouseOrder>(m => new WarehouseSendActor.OrderIsSent { WarehouseId = m.WarehouseId });
            }

            [Fact]
            public void Should_Return_Order_Processed_When_Processing_Successful()
            {
                // Act
                Subject.Tell(message);

                // Assert
                Kit.ExpectMsg<ProcessingCoordinatorActor.OrderIsProcessed>();

                this.progHttpRequestActor.GetRequest<HttpRequestActor.Get>().Should().BeEquivalentTo(
                    new HttpRequestActor.Get
                    {
                        ServiceUrl = "http://order-service:5000",
                        Path = "/private/order/5"
                    });

                this.progHttpRequestActor.GetRequest<HttpRequestActor.Post>().Should().BeEquivalentTo(
                    new HttpRequestActor.Post
                    {
                        ServiceUrl = "http://product-service:5000",
                        Path = $"/private/product/list",
                        Data = new
                        {
                            ProductIds = new[] { 5, 10 }
                        }
                    });

                this.progWarehouseSendActor.GetRequests<Models.WarehouseOrder>().Should().BeEquivalentTo(
                    new[]
                    {
                        new Models.WarehouseOrder
                        {
                            Id = "5-1",
                            OrderId = 5,
                            WarehouseId = 1,
                            OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture),
                            Lines = new[]
                            {
                                new Models.WarehouseOrder.OrderLine { Line = 1, ProductId = 5, Description = "product 1", Amount = 5 }
                            }
                        },
                        new Models.WarehouseOrder
                        {
                            Id = "5-2",
                            OrderId = 5,
                            WarehouseId = 2,
                            OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture),
                            Lines = new[]
                            {
                                new Models.WarehouseOrder.OrderLine { Line = 2, ProductId = 10, Description = "product 2", Amount = 2 }
                            }
                        }
                    });
            }

            [Fact]
            public void Should_Return_Order_Processing_Failed_When_Order_Service_Returns_Non_OK()
            {
                // Arrange
                this.progHttpRequestActor.SetResponseForRequest<HttpRequestActor.Get>(_ =>
                    new HttpRequestActor.ReceivedNonSuccessServiceResponse(HttpStatusCode.BadGateway, Array.Empty<byte>()));

                // Act
                Subject.Tell(message);

                // Assert
                Kit.ExpectMsg<ProcessingCoordinatorActor.OrderFailedToProcess>();
            }

            [Fact]
            public void Should_Return_Order_Processing_Failed_When_Order_Service_Communication_Failure()
            {
                // Arrange
                this.progHttpRequestActor.SetResponseForRequest<HttpRequestActor.Get>(_ =>
                    new HttpRequestActor.ServiceRequestFailed(HttpRequestActor.ServiceRequestFailed.FailureReason.ServiceRequestTimeout));

                // Act
                Subject.Tell(message);

                // Assert
                Kit.ExpectMsg<ProcessingCoordinatorActor.OrderFailedToProcess>();
            }

            [Fact]
            public void Should_Return_Order_Processing_Failed_When_Product_Service_Returns_Non_OK()
            {
                // Arrange
                this.progHttpRequestActor.SetResponseForRequest<HttpRequestActor.Post>(_ =>
                    new HttpRequestActor.ReceivedNonSuccessServiceResponse(HttpStatusCode.BadGateway, Array.Empty<byte>()));

                // Act
                Subject.Tell(message);

                // Assert
                Kit.ExpectMsg<ProcessingCoordinatorActor.OrderFailedToProcess>();
            }

            [Fact]
            public void Should_Return_Order_Processing_Failed_When_Product_Service_Communication_Failure()
            {
                // Arrange
                this.progHttpRequestActor.SetResponseForRequest<HttpRequestActor.Post>(_ =>
                    new HttpRequestActor.ServiceRequestFailed(HttpRequestActor.ServiceRequestFailed.FailureReason.ServiceRequestTimeout));

                // Act
                Subject.Tell(message);

                // Assert
                Kit.ExpectMsg<ProcessingCoordinatorActor.OrderFailedToProcess>();
            }

            [Fact]
            public void Should_Return_Order_Processing_Failed_When_One_Warehouse_Order_Is_Not_Sent()
            {
                // Arrange
                this.progWarehouseSendActor.SetResponseForRequest<Models.WarehouseOrder>(m =>
                    m.WarehouseId == 1 ?
                        (WarehouseSendActor.IResponse)new WarehouseSendActor.OrderIsSent { WarehouseId = m.WarehouseId } :
                        (WarehouseSendActor.IResponse)new WarehouseSendActor.OrderIsNotSent { WarehouseId = m.WarehouseId });

                // Act
                Subject.Tell(message);

                // Assert
                Kit.ExpectMsg<ProcessingCoordinatorActor.OrderIsPartiallyProcessed>();
            }

            [Fact]
            public void Should_Return_Order_Processing_Failed_When_All_Warehouse_Orders_Are_Not_Sent()
            {
                // Arrange
                this.progWarehouseSendActor.SetResponseForRequest<Models.WarehouseOrder>(m =>
                    new WarehouseSendActor.OrderIsNotSent { WarehouseId = m.WarehouseId });

                // Act
                Subject.Tell(message);

                // Assert
                Kit.ExpectMsg<ProcessingCoordinatorActor.OrderFailedToProcess>();
            }

            private ProcessingUnitActor.StartProcessing message = new ProcessingUnitActor.StartProcessing
            {
                OrderId = 5,
                OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture)
            };
        }
    }
}
