
using Law_Model.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Law_Model.Static_file.Static_datas;

namespace Law_Model.Utility
{
    public static class SeedRolesAndAdmin
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Ensure all roles exist
            foreach (var role in Enum.GetNames(typeof(UserRole)))
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create default admin if not exists
            var adminEmail = "admin@lawfirm.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "Power",
                    Role = UserRole.Admin,
                    EmailConfirmed = true
                };

                // Modified admin password to meet requirements
                var result = await userManager.CreateAsync(adminUser, "Admin@123456");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, UserRole.Admin.ToString());
                }
                else
                {
                    // Log or handle errors
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Admin user creation failed: {errors}");
                }
            }
        }
    }
}