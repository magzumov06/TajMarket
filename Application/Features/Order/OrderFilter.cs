using Domain.Enums;
using Domain.Filters;

namespace Application.Features.Order;

public class OrderFilter : BaseFilter
{
    public OrderStatus? Status { get; set; }   
    public string? SortBy { get; set; }       
}