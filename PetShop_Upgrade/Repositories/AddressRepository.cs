using Microsoft.EntityFrameworkCore;
using PetShop_Upgrade.Data;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;

namespace PetShop_Upgrade.Repositories
{
    public class AddressRepository : Repository<Address>, IAddressRepository
    {
        public AddressRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Address>> GetAllAddressesByMemberIdAsync(int memberId)
        {
            return await _context.Addresses
                .Where(a => a.MemberId == memberId)
                .ToListAsync();
        }
    }
}
