using LabelVerify.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace LabelVerify.Web.Data
{
    public class IdentitySeed
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles =
            [
                "Administrator", "Supervisor", "Reviewer"
            ];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "bcole@bcoleonline.com";

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    DisplayName = "System Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Password123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Administrator");
                }
            }

            var revEmail = "rev1@test.com";

            var rev = await userManager.FindByEmailAsync(revEmail);

            if (rev == null)
            {
                rev = new ApplicationUser
                {
                    UserName = revEmail,
                    Email = revEmail,
                    DisplayName = "Reviewer #1",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(rev, "Password123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Reviewer");
                }
            }
        }

        private static async Task CreateUserIfMissing(UserManager<ApplicationUser> userManager,
            string email, string displayName, string role)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user != null)
            {
                return;
            }

            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = displayName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "Password123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}