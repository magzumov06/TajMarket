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

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> AddToRoleAsync(int userId, string role, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user == null)
            return (false, new[] { "User not found" });

        var result = await userManager.AddToRoleAsync(user, role);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
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
    
    public async Task<(bool Succeeded, IEnumerable<string> Errors)> CreateUserAsync(User user, string password, CancellationToken cancellationToken)
    {
        var result = await userManager.CreateAsync(user, password);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    public async Task<User?> FindUserByEmailAsync(string email, CancellationToken cancellationToken) =>
        await userManager.FindByEmailAsync(email);

    public async Task DeleteUserAsync(User user, CancellationToken cancellationToken) =>
        await userManager.DeleteAsync(user);

    public async Task<bool> IsEmailConfirmedAsync(User user, CancellationToken cancellationToken) =>
        await userManager.IsEmailConfirmedAsync(user);

    public async Task<bool> IsLockedOutAsync(User user, CancellationToken cancellationToken) =>
        await userManager.IsLockedOutAsync(user);

    public async Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken) =>
        await userManager.CheckPasswordAsync(user, password);

    public async Task RecordAccessFailureAsync(User user, CancellationToken cancellationToken) =>
        await userManager.AccessFailedAsync(user);

    public async Task ResetAccessFailedCountAsync(User user, CancellationToken cancellationToken) =>
        await userManager.ResetAccessFailedCountAsync(user);

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> ChangePasswordAsync(User user, string oldPassword, string newPassword, CancellationToken cancellationToken)
    {
        var result = await userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }
}