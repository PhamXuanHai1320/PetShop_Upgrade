using Microsoft.EntityFrameworkCore;
using PetShop_Upgrade.Data;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive == IsActive.ACTIVE)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesByNameAsync(string name)
        {
            return await _context.Categories
                .Where(c => c.Name.Contains(name) && c.IsActive == IsActive.ACTIVE)
                .ToListAsync();
        }
    }
}
