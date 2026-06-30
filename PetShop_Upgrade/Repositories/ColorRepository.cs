using Microsoft.EntityFrameworkCore;
using PetShop_Upgrade.Data;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Repositories
{
    public class ColorRepository : Repository<Color>, IColorRepository
    {
        public ColorRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Color>> GetColorsByProductIdAsync(int productId)
        {
            var colors = await _context.Colors
                .Where(c => c.ProductColors.Any(pc => pc.ProductId == productId) && c.IsActive == IsActive.ACTIVE)
                .Include(c => c.ProductColors.Where(pc => pc.ProductId == productId)) // Nạp kèm dữ liệu đã lọc
                .ToListAsync();
            return colors;
        }
        public async Task<IEnumerable<Color>> GetColorsByNameAsync(string colorName)
        {
            var colors = await _context.Colors
                .Where(c => c.ColorName.Contains(colorName) && c.IsActive == IsActive.ACTIVE)
                .ToListAsync();
            return colors;
        }

        public async Task<IEnumerable<Color>> GetAllColorsAsync()
        {
            return await _context.Colors
                .Where(c => c.IsActive == IsActive.ACTIVE)
                .ToListAsync();
        }

        public async Task<Color> GetColorByIdAsync(int colorId)
        {
            return await _context.Colors
                .FirstOrDefaultAsync(c => c.Id == colorId && c.IsActive == IsActive.ACTIVE);
        }

        public async Task<bool> HasProductColorByColorIdAsync(int colorId)
        {
            return await _context.ProductColors
                .AnyAsync(pc => pc.ColorId == colorId);
        }
    }
}
