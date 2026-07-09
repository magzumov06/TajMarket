using Domain.Entities.ProductEntity;
using Domain.Entities.UserEntity;

namespace Domain.Entities;

public class WishlistItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}