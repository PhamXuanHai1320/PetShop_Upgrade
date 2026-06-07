using Microsoft.AspNetCore.Identity;
using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Data
{
    public class DbSeeder
    {
        public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<Member>>();
            var adminUserName = "admin";
            var adminUser = await userManager.FindByNameAsync(adminUserName);
            // Nếu chưa có tài khoản admin này trong DB thì mới tạo
            if (adminUser == null)
            {
                var newAdmin = new Member
                {
                    UserName = adminUserName,
                    Email = "admin@petshop.com",
                    FirstName = "Hệ",
                    LastName = "Thống"
                };

                // UserManager TỰ ĐỘNG băm mật khẩu 
                var createPowerUser = await userManager.CreateAsync(newAdmin, "Admin@123456");
                if (createPowerUser.Succeeded)
                {
                    // Gán quyền Admin
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }
    }
}
