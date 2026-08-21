using Domain.Filters;

namespace Application.Features.Courier;

public class CourierHistoryFilter : BaseFilter
{
    public string? SortBy { get; set; }
}