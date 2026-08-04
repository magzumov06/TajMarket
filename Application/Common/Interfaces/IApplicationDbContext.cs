using Microsoft.EntityFrameworkCore;
using Domain.Entities.CategoryEntity;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}