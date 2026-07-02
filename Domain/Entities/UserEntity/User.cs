using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class User : IdentityUser<int>
{
    public string FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? AvatarPublickId { get; set; }
    public DateTime CreatedAt { get; set; } =  DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}