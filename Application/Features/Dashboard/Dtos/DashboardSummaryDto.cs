namespace Application.Features.Dashboard.Dtos;

public record DashboardSummaryDto(
    decimal TodayRevenue,
    int TodayOrdersCount,
    decimal ThisMonthRevenue,
    int ThisMonthOrdersCount,
    int PendingOrdersCount,
    int ActiveCouriersCount,
    int PendingReturnRequestsCount,
    int TotalActiveProducts,
    int TotalActiveSellers,
    int TotalCustomers
);