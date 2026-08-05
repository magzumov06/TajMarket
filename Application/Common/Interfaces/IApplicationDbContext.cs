using Domain.Entities.AddressEntity;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.CategoryEntity;
using Domain.Entities.OrderEntity;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Address> Addresses { get; }
    DbSet<Order> Orders { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}