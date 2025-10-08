using Microsoft.AspNetCore.Identity;

namespace LugaPasal.Validation
{
    public class RoleSeeder
    {
        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = ["Admin", "User", "Vendor"];

            foreach (string r in roles)
            {
                var result = await roleManager.RoleExistsAsync(r);
                if (!result)
                {
                    await roleManager.CreateAsync(new IdentityRole(r));
                }
            }

        }
    }
}
