using Domain.Entities.UserEntity;
using Domain.Enums;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public static class Seed
{
    public static async Task SeedAdmin(
        UserManager<User> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        AdminSeedSettings settings) 
    {
        if (!await roleManager.RoleExistsAsync(Role.Admin.ToString()))
            await roleManager.CreateAsync(new IdentityRole<int>("Admin"));

        var user = userManager.Users.FirstOrDefault(x => x.Email == settings.Email);

        if (user == null)
        {
            var newUser = new User
            {
                FullName = "Admin",
                UserName = settings.Email,
                Email = settings.Email,
                PhoneNumber = settings.PhoneNumber,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var res = await userManager.CreateAsync(newUser, settings.Password);

            if (res.Succeeded)
                await userManager.AddToRoleAsync(newUser, Role.Admin.ToString());
        }
    }

    public static async Task<bool> SeedRole(RoleManager<IdentityRole<int>> roleManager)
    {
        var newRole = new List<IdentityRole<int>>()
        {
            new(Role.Admin.ToString()),
            new(Role.Customer.ToString()),
            new(Role.Seller.ToString()),
            new(Role.Courier.ToString())
        };
        var roles = await roleManager.Roles.ToListAsync();
        foreach (var role in newRole)
        {
            if (roles.Any(r => r.Name == role.Name))
                continue;
            await roleManager.CreateAsync(role);
        }

        return true;
    }
}