using Domain.Enums;

namespace Domain.Filters;

public class OrderFilter : BaseFilter
{
    public OrderStatus? Status { get; init; }   // ихтиёрӣ: фақат як статусро нишон диҳед
    public string? SortBy { get; init; }        // "date_asc" ё "date_desc" (default: date_desc)
}