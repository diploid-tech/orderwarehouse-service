using System.Globalization;
using Akka.Actor;
using Akka.Event;
using Akka.Logger.Serilog;
using AutoMapper;
using Avanti.Core.Http;
using Avanti.Core.Microservice.Actors;
using Microsoft.Extensions.Options;

namespace Avanti.OrderWarehouseService.WarehouseOrder
{
    public partial class WarehouseSendActor : ReceiveActor
    {
        private readonly ILoggingAdapter logger = Context.GetLogger<SerilogLoggingAdapter>();
        private readonly IActorRef httpRequestActor;
        private readonly ServiceSettings serviceSettings;
        private int warehouseId;

        public WarehouseSendActor(
            IActorProvider<HttpRequestActor> httpRequestActorProvider,
            IOptions<ServiceSettings> serviceSettings)
        {
            this.httpRequestActor = httpRequestActorProvider.Get();
            this.serviceSettings = serviceSettings.Value;

            Receive<Models.WarehouseOrder>(Handle);
        }

        private void Handle(Models.WarehouseOrder m)
        {
            this.warehouseId = m.WarehouseId;
            if (serviceSettings.WarehouseServiceUris.TryGetValue(this.warehouseId.ToString(CultureInfo.InvariantCulture), out var warehouseServiceUri))
            {
                Become(ReceiveResponseState);
                this.httpRequestActor.Tell(
                    new HttpRequestActor.Post
                    {
                        ServiceUrl = warehouseServiceUri.Port == 443 ? $"https://{warehouseServiceUri.Host}" : $"http://{warehouseServiceUri.Host}:{warehouseServiceUri.Port}",
                        Path = "/private/order",
                        Data = m
                    });
            }
            else
            {
                this.logger.Error($"Uri location of warehouse {this.warehouseId} is not known!");
                Parent.Tell(new OrderIsNotSent { WarehouseId = this.warehouseId });
                Context.Stop(Context.Self);
            }
        }

        private void ReceiveResponseState()
        {
            Receive<HttpRequestActor.ReceivedSuccessServiceResponse>(r =>
            {
                Parent.Tell(new OrderIsSent { WarehouseId = this.warehouseId });
                Context.Stop(Context.Self);
            });
            Receive<HttpRequestActor.ReceivedNonSuccessServiceResponse>(r =>
            {
                this.logger.Error($"Unexpected response from warehouse-service {this.warehouseId}: {r.StatusCode}");
                Parent.Tell(new OrderIsNotSent { WarehouseId = this.warehouseId });
                Context.Stop(Context.Self);
            });
            ReceiveAny(_ =>
            {
                Parent.Tell(new OrderIsNotSent { WarehouseId = this.warehouseId });
                Context.Stop(Context.Self);
            });
        }

        protected virtual IActorRef Parent { get => Context.Parent; }
    }
}
