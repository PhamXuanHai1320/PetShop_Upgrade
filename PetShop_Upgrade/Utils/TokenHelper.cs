using PetShop_Upgrade.Utils.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace PetShop_Upgrade.Utils
{
    public class TokenHelper : ITokenHelper
    {
        public string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        public string HashRefreshToken(string plainTextToken)
        {
            using var sha256 = SHA256.Create();
            // Đổi chuỗi text thành mảng byte
            var bytes = Encoding.UTF8.GetBytes(plainTextToken);
            // Tiến hành băm 1 chiều
            var hashBytes = sha256.ComputeHash(bytes);
            // Trả về chuỗi đã băm (lại dùng Base64 để hiển thị cho đẹp)
            return Convert.ToBase64String(hashBytes);
        }
    }
}
