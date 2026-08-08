using Application.Common.Interfaces;
using Domain.Entities.UserEntity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public class IdentityService(UserManager<User> userManager) : IIdentityService
{
    public async Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user != null;
    }

    public async Task<bool> IsInRoleAsync(int userId, string role, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user != null && await userManager.IsInRoleAsync(user, role);
    }

    public async Task AddToRoleAsync(int userId, string role, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user != null)
            await userManager.AddToRoleAsync(user, role);
    }

    public async Task<User?> FindUserByIdAsync(int userId, CancellationToken cancellationToken) =>
        await userManager.FindByIdAsync(userId.ToString());

    public async Task<IList<string>> GetUserRolesAsync(User user, CancellationToken cancellationToken) =>
        await userManager.GetRolesAsync(user);

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateUserAsync(User user,
        CancellationToken cancellationToken)
    {
        var result = await userManager.UpdateAsync(user);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }
    
    public async Task RemoveFromRoleAsync(int userId, string role, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user != null)
            await userManager.RemoveFromRoleAsync(user, role);
    }
}