using Domain.Entities.AddressEntity;
using Domain.Entities.CartEntity;
using Domain.Entities.OrderEntity;
using Domain.Entities.ReviewEntity;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class User : IdentityUser<int>
{
    public string FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? AvatarPublicId { get; set; }   
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public ICollection<Order> Orders { get; set; }
    public ICollection<Review> Reviews { get; set; }
    public ICollection<Address> Addresses { get; set; }
    public Cart? Cart { get; set; }
    public SellerProfile? SellerProfile { get; set; }
    public ICollection<Notification> Notifications { get; set; }
    public ICollection<WishlistItem> WishlistItems { get; set; }
}