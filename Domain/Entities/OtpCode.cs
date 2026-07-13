using Domain.Entities.UserEntity;

namespace Domain.Entities;

public class OtpCode
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public string Code { get; set; } = string.Empty;

    public int AttemptCount { get; set; }
    
    public DateTime ExpireAt { get; set; }

    public bool IsUsed { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}