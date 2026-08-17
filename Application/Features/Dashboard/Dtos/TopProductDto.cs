namespace Application.Features.Dashboard.Dtos;

public record TopProductDto(int ProductId, string Name, string? ImageUrl, int TotalSold, decimal TotalRevenue);