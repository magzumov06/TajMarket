using Domain.Entities.UserEntity;

namespace Application.Common.Interfaces;

public interface IIdentityService
{
    Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken);
    Task<bool> IsInRoleAsync(int userId, string role, CancellationToken cancellationToken);
    Task AddToRoleAsync(int userId, string role, CancellationToken cancellationToken);
    
    Task<User?> FindUserByIdAsync(int userId, CancellationToken cancellationToken);
    Task<IList<string>> GetUserRolesAsync(User user, CancellationToken cancellationToken);
    Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateUserAsync(User user, CancellationToken cancellationToken);
    
    Task RemoveFromRoleAsync(int userId, string role, CancellationToken cancellationToken);
}