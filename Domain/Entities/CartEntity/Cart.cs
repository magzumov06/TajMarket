using Domain.Entities.UserEntity;

namespace Domain.Entities.CartEntity;

public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<CartItem> Items { get; set; }
}