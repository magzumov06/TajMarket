using Domain.Entities;
using Domain.Entities.AddressEntity;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.CategoryEntity;
using Domain.Entities.OrderEntity;
using Domain.Entities.ProductEntity;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Address> Addresses { get; }
    DbSet<Order> Orders { get; }
    DbSet<Product> Products { get; }              
    DbSet<WishlistItem> WishlistItems { get; }
    DbSet<Coupon> Coupons { get; }   

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}