using PetShop_Upgrade.DTOS.Members.Admin;
using PetShop_Upgrade.Models;
using System.Security.Claims;

namespace PetShop_Upgrade.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateTokenAsync(MemberDTO memberDTO);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
