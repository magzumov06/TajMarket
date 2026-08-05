using Domain.Enums;
using Domain.Filters;

namespace Application.Features.Order;

public class OrderFilter : BaseFilter
{
    public OrderStatus? Status { get; init; }   
    public string? SortBy { get; init; }       
}