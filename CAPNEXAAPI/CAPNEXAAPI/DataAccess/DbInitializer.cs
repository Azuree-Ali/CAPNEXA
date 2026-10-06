using CAPNEXAAPI.Models;
using CAPNEXAAPI.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CAPNEXAAPI.DataAccess;

public static class DbInitializer
{
    private static readonly string[] Roles =
    [
        CD.ADMIN_ROLE,
        CD.STUDENT_ROLE,
        CD.SUPERVISOR_ROLE
    ];

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        await dbContext.Database.MigrateAsync();

        foreach (var roleName in Roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                EnsureSucceeded(result, $"creating role '{roleName}'");
            }
        }

        var email = configuration["SeedAdmin:Email"];
        var userName = configuration["SeedAdmin:UserName"];
        var password = configuration["SeedAdmin:Password"];
        var firstName = configuration["SeedAdmin:FirstName"];
        var lastName = configuration["SeedAdmin:LastName"];

        if (new[] { email, userName, password, firstName, lastName }.Any(string.IsNullOrWhiteSpace))
        {
            logger.LogWarning("SeedAdmin settings are incomplete; roles were seeded, but no Admin account was created.");
            return;
        }

        var admin = await userManager.FindByEmailAsync(email!);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName!,
                LastName = lastName!,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(admin, password!);
            EnsureSucceeded(createResult, "creating the seeded Admin account");
        }

        if (!await userManager.IsInRoleAsync(admin, CD.ADMIN_ROLE))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, CD.ADMIN_ROLE);
            EnsureSucceeded(roleResult, "assigning the Admin role to the seeded account");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed while {operation}: {errors}");
        }
    }
}
