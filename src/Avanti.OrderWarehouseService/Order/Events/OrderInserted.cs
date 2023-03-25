using System.Globalization;
using Avanti.Core.EventStream;
using Avanti.Core.EventStream.Model;

namespace Avanti.OrderWarehouseService.Order.Events;

[PlatformEventDescription("Avanti.Order.OrderInserted", EventProcessingTypeEnum.Incoming, "orders")]
public class OrderInserted : IPlatformEvent
{
    public string SubjectId => this.Id.ToString(CultureInfo.InvariantCulture);

    public int Id { get; set; }

    public DateTimeOffset OrderDate { get; set; }
}
