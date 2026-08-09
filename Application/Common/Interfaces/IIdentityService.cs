using Domain.Entities.UserEntity;

namespace Application.Common.Interfaces;

public interface IIdentityService
{
    Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken);
    Task<bool> IsInRoleAsync(int userId, string role, CancellationToken cancellationToken);
    
    Task<User?> FindUserByIdAsync(int userId, CancellationToken cancellationToken);
    Task<IList<string>> GetUserRolesAsync(User user, CancellationToken cancellationToken);
    Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateUserAsync(User user, CancellationToken cancellationToken);
    
    Task RemoveFromRoleAsync(int userId, string role, CancellationToken cancellationToken);
    
    Task<(bool Succeeded, IEnumerable<string> Errors)> CreateUserAsync(User user, string password, CancellationToken cancellationToken);
    Task<User?> FindUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task DeleteUserAsync(User user, CancellationToken cancellationToken);
    Task<bool> IsEmailConfirmedAsync(User user, CancellationToken cancellationToken);
    Task<bool> IsLockedOutAsync(User user, CancellationToken cancellationToken);
    Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken);
    Task RecordAccessFailureAsync(User user, CancellationToken cancellationToken);
    Task ResetAccessFailedCountAsync(User user, CancellationToken cancellationToken);
    Task<(bool Succeeded, IEnumerable<string> Errors)> ChangePasswordAsync(User user, string oldPassword, string newPassword, CancellationToken cancellationToken);
    
    Task<(bool Succeeded, IEnumerable<string> Errors)> AddToRoleAsync(int userId, string role, CancellationToken cancellationToken);
}