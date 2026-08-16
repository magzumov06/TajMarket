using Domain.Enums;

namespace Domain.Entities.OrderEntity;

public class ReturnRequest
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int UserId { get; set; }
    public string Reason { get; set; } = null!;
    public ReturnStatus Status { get; set; } = ReturnStatus.Requested;
    public string? AdminComment { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}