using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using Akka.Logger.Serilog;
using AutoMapper;
using Avanti.Core.Http;
using Avanti.Core.Microservice;
using Avanti.Core.Microservice.Actors;
using Avanti.Core.Microservice.AkkaSupport;
using Avanti.OrderWarehouseService.Order;
using Microsoft.Extensions.Options;
using Failure = Avanti.Core.Microservice.Failure;

namespace Avanti.OrderWarehouseService.WarehouseOrder
{
    public partial class ProcessingUnitActor : ReceiveActor
    {
        private readonly ILoggingAdapter logger = Context.GetLogger<SerilogLoggingAdapter>();
        private readonly IActorRef httpRequestActor;
        private readonly ServiceSettings serviceSettings;
        private readonly IMapper mapper;
        private IActorRef? initiatorActorRef;
        private int? orderId;
        private Models.OrderModel? order;
        private IEnumerable<Models.WarehouseOrderModel>? warehouseOrders;

        public ProcessingUnitActor(
            IActorProvider<HttpRequestActor> httpRequestActorProvider,
            IOptions<ServiceSettings> serviceSettings,
            IMapper mapper)
        {
            this.httpRequestActor = httpRequestActorProvider.Get();
            this.serviceSettings = serviceSettings.Value;
            this.mapper = mapper;

            Receive<StartProcessing>(Handle);
        }

        private void Handle(StartProcessing m)
        {
            this.logger.Info($"Start processing of order {m.OrderId}");

            Context.SetReceiveTimeout(TimeSpan.FromSeconds(120));
            this.initiatorActorRef = Context.Sender;
            this.orderId = m.OrderId;

            Become(ReceiveOrderDetailsState);
            Uri? orderServiceUri = this.serviceSettings.OrderServiceUri ?? new Uri("http://unknown/");
            this.httpRequestActor.Tell(
                new HttpRequestActor.Get
                {
                    ServiceUrl = orderServiceUri.Port == 443 ? $"https://{orderServiceUri.Host}" : $"http://{orderServiceUri.Host}:{orderServiceUri.Port}",
                    Path = $"/private/order/{m.OrderId}"
                });
        }

        private void ReceiveOrderDetailsState()
        {
            this.logger.Debug($"Wait for retrieving order details of {this.orderId}");
            Receive<HttpRequestActor.ReceivedSuccessServiceResponse>(r =>
            {
                this.order = r.ResponseToObjectOf<Models.OrderModel>();
                Uri? productServiceUri = this.serviceSettings.ProductServiceUri ?? new Uri("http://unknown/");
                Become(ReceiveProductDetailsState);
                this.httpRequestActor.Tell(
                    new HttpRequestActor.Post
                    {
                        ServiceUrl = productServiceUri.Port == 443 ? $"https://{productServiceUri.Host}" : $"http://{productServiceUri.Host}:{productServiceUri.Port}",
                        Path = "/private/product/list",
                        Data = new
                        {
                            ProductIds = this.order.Lines.Select(line => line.ProductId)
                        }
                    });
            });
            Receive<HttpRequestActor.ReceivedNonSuccessServiceResponse>(r =>
            {
                this.logger.Error($"Unexpected response from order-service: {r.StatusCode}");
                TellFailureAndKillSelf();
            });
            ReceiveAny(_ => TellFailureAndKillSelf());
        }

        private void ReceiveProductDetailsState()
        {
            Receive<HttpRequestActor.ReceivedSuccessServiceResponse>(r =>
            {
                string? data = r.ResponseToString();
                IEnumerable<Models.Product>? products = r.ResponseToObjectOf<IEnumerable<Models.Product>>();
                this.order!.Lines = this.order.Lines.Select(l =>
                {
                    l.Product = products.FirstOrDefault(p => p.Id == l.ProductId);
                    return l;
                });

                if (this.order.Lines.Any(l => l.Product == null))
                {
                    this.logger.Warning($"Order has invalid products on line(s): {string.Join(", ", this.order.Lines.Where(l => l.Product == null).Select(l => l.Line))}");
                    TellFailureAndKillSelf();
                }

                this.warehouseOrders = this.mapper.Map<IEnumerable<Models.WarehouseOrderModel>>(
                    this.order.Lines.GroupBy(o => o.Product!.WarehouseId).Select(g => (this.order, g)));

                Become(ReceiveWarehouseResponseState);
                foreach (Models.WarehouseOrderModel? warehouseOrder in this.warehouseOrders)
                {
                    IActorRef? actor = CreateWarehouseSendActorRef($"warehouse-{warehouseOrder.WarehouseId}");
                    actor.Tell(warehouseOrder);
                }
            });
            Receive<HttpRequestActor.ReceivedNonSuccessServiceResponse>(r =>
            {
                this.logger.Error($"Unexpected response from product-service: {r.StatusCode}");
                TellFailureAndKillSelf();
            });
            ReceiveAny(_ => TellFailureAndKillSelf());
        }

        private void ReceiveWarehouseResponseState()
        {
            var validations = this.warehouseOrders!.ToDictionary(o => o.WarehouseId, _ => default(Result));

            void CheckAllValidations()
            {
                if (!validations.Values.Any(v => v == default(Result)))
                {
                    // TODO: should same the warehouse order and it's status
                    if (validations.Values.All(v => v is IsSuccess))
                    {
                        this.logger.Info($"Order {this.orderId} is processed");
                        this.initiatorActorRef.Tell(new ProcessingCoordinatorActor.OrderIsProcessed());
                    }
                    else if (validations.Values.All(v => v is IsFailure))
                    {
                        this.logger.Warning($"Order {this.orderId} partially processed");
                        this.initiatorActorRef.Tell(new ProcessingCoordinatorActor.OrderFailedToProcess());
                    }
                    else
                    {
                        this.logger.Error($"Order {this.orderId} failed to process");
                        this.initiatorActorRef.Tell(new ProcessingCoordinatorActor.OrderIsPartiallyProcessed());
                    }

                    Context.Stop(Context.Self);
                }
            }

            Receive<WarehouseSendActor.OrderIsSent>(m =>
            {
                validations[m.WarehouseId] = new Success();
                CheckAllValidations();
            });
            Receive<WarehouseSendActor.OrderIsNotSent>(m =>
            {
                validations[m.WarehouseId] = new Failure();
                CheckAllValidations();
            });
        }

        protected virtual IActorRef CreateWarehouseSendActorRef(string actorName) =>
            Context.ActorOfWithDI<WarehouseSendActor>(actorName);

        private void TellFailureAndKillSelf()
        {
            this.initiatorActorRef.Tell(new ProcessingCoordinatorActor.OrderFailedToProcess());
            Context.Stop(Context.Self);
        }
    }
}
