using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Features.Orders.Dtos.Shared;
public record OrderItemDto(
    Guid MenuItemId,
    string FoodName,
    int Quantity,
    decimal UnitPrice,
    decimal Total);