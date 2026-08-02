namespace Order.Application.Features.Orders.Dtos.GetOrderStatistics;

public record OrderStatisticsDto(
    int TotalOrders,
    int CompletedOrders,
    int CancelledOrders,
    decimal TotalSales,
    double AveragePreparationTimeMinutes);