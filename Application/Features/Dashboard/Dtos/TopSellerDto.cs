namespace Application.Features.Dashboard.Dtos;

public record TopSellerDto(int SellerProfileId, string StoreName, int TotalOrders, decimal TotalRevenue);