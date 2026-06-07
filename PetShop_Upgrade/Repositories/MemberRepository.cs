using Microsoft.EntityFrameworkCore;
using PetShop_Upgrade.Data;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;

namespace PetShop_Upgrade.Repositories
{
    public class MemberRepository : Repository<Member>, IMemberRepository
    {
        public MemberRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> CheckDuplicateMember(string email, string username, int id)
        {
            return await _context.Members
                .AnyAsync(m => m.Id != id && (m.Email == email || m.UserName == username));
        }

        public async Task<bool> CheckDuplicateMember(string email, string username)
        {
            return await _context.Members
                .AnyAsync(m => m.Email == email || m.UserName == username);
        }

        public async Task<Member> GetMemberByEmailAsync(string email)
        {
            return await _context.Members
                .FirstOrDefaultAsync(m => m.Email == email);
        }
    }
}
