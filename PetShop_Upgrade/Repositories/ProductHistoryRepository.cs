using Microsoft.EntityFrameworkCore;
using PetShop_Upgrade.Data;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;

namespace PetShop_Upgrade.Repositories
{
    public class ProductHistoryRepository : Repository<ProductHistory>, IProductHistoryRepository
    {
        public ProductHistoryRepository(ApplicationDbContext context) : base(context) 
        { 
        }
        public async Task<int> GetLatestVersionAsync(int productId)
        {
            return await _context.ProductHistories
                .Where(ph => ph.ProductId == productId)
                .MaxAsync(ph => (int?)ph.Version) ?? 0;
        }
    }
}
