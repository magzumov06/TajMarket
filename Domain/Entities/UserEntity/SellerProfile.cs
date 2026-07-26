using Domain.Entities.ProductEntity;

namespace Domain.Entities.UserEntity;

public class SellerProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }

    public string StoreName { get; set; }
    public string? StoreDescription { get; set; }
    public string? StoreLogoUrl { get; set; }
    public string? StoreLogoPublicId { get; set; }
    public bool IsVerified { get; set; }
    public decimal Rating { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public ICollection<Product> Products { get; set; }
}