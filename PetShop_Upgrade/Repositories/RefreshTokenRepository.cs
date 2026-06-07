using Microsoft.EntityFrameworkCore;
using PetShop_Upgrade.Data;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;

namespace PetShop_Upgrade.Repositories
{
    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<RefreshToken?> FindValidTokenAsync(string hashToken)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.HashToken == hashToken && t.ExpiresAt > DateTime.UtcNow && !t.IsRevoked);
        }

        public async Task<RefreshToken?> FindValidTokenOrMemberIdAsync(string hashToken, int memberId)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.HashToken == hashToken && t.MemberId == memberId);
        }

        public async Task RevokeAllMemberTokensAsync(int memberId)
        {
            var tokens = await _context.RefreshTokens
                .Where(t => t.MemberId == memberId && !t.IsRevoked)
                .ToListAsync(); // vẫn load 1 lần

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }
        }

        public async Task RevokeTokenAsync(RefreshToken token)
        {
            token.IsRevoked = true;
            _context.RefreshTokens.Update(token);
        }
    }
}
