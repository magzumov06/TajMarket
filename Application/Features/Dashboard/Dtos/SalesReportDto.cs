namespace Application.Features.Dashboard.Dtos;

public record SalesReportPointDto(DateTime Date, decimal Revenue, int OrdersCount);

public record SalesReportDto(List<SalesReportPointDto> Points, decimal TotalRevenue, int TotalOrders);