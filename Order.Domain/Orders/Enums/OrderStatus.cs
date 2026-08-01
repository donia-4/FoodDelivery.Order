using System.Text.Json.Serialization;

namespace Order.Domain.Orders.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus
{
    Pending,
    Accepted,
    Rejected,
    Preparing,
    Ready,
    OutForDelivery,
    Delivered,
    Completed,
    Cancelled
}