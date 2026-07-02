using Microsoft.IdentityModel.Tokens;
using PetShop_Upgrade.DTOS.Members.Admin;
using PetShop_Upgrade.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Claims;
using System.Text;

namespace PetShop_Upgrade.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly SymmetricSecurityKey _key;
        private readonly string _issuer;
        private readonly string _auience;
        private readonly int _expires;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            _issuer = _configuration["Jwt:Issuer"];
            _auience = _configuration["Jwt:Audience"];
            _expires = int.Parse(_configuration["Jwt:AccessTokenExpireMinutes"]);
        }
        public string GenerateTokenAsync(MemberDTO memberDTO)
        {
            var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim("id", memberDTO.Id.ToString()),
                new Claim("username", memberDTO.Username),
                new Claim("role", memberDTO.Role)
            };
            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: _issuer,
                audience: _auience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expires),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _auience,
                IssuerSigningKey = _key,
                ValidateLifetime = false  // ← Cho phép token expired
            };

            try
            {
                var principal = new JwtSecurityTokenHandler()
                    .ValidateToken(token, tokenValidationParams, out var validatedToken);

                // Đảm bảo đúng thuật toán
                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
                {
                    return null;
                }    
                    

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
