using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> FindValidTokenAsync(string hashToken);
        Task<RefreshToken?> FindValidTokenOrMemberIdAsync(string hashToken, int memberId);
        Task RevokeTokenAsync(RefreshToken token);
        Task RevokeAllMemberTokensAsync(int userId);
    }
}
