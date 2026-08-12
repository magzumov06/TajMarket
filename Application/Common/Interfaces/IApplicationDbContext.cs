using Domain.Entities;
using Domain.Entities.AddressEntity;
using Domain.Entities.CartEntity;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.CategoryEntity;
using Domain.Entities.OrderEntity;
using Domain.Entities.PaymentEntity;
using Domain.Entities.ProductEntity;
using Domain.Entities.ReviewEntity;
using Domain.Entities.UserEntity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Address> Addresses { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Product> Products { get; }              
    DbSet<WishlistItem> WishlistItems { get; }
    DbSet<Coupon> Coupons { get; }   
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Courier> Couriers { get; }
    DbSet<SellerProfile> SellerProfiles { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<ProductImage> ProductImages { get; }   
    DbSet<Review> Reviews { get; } 
    DbSet<User> Users { get; }
    DbSet<RevokedToken> RevokedTokens { get; }
    
    DbSet<IdentityUserRole<int>> UserRoles { get; } 
    DbSet<IdentityRole<int>> Roles { get; } 
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);

}