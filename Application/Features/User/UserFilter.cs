using Domain.Filters;

namespace Application.Features.User;

public class UserFilter : BaseFilter
{
    public string? SearchTerm { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
}