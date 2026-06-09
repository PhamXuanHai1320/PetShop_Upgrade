using Microsoft.EntityFrameworkCore;
using PetShop_Upgrade.Data;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;

namespace PetShop_Upgrade.Repositories
{
    public class ColorRepository : Repository<Color>, IColorRepository
    {
        public ColorRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Color>> GetColorsByProductId(int productId)
        {
            var colors = await _context.Colors
                .Where(c => c.ProductColors.Any(pc => pc.ProductId == productId))
                .Include(c => c.ProductColors.Where(pc => pc.ProductId == productId)) // Nạp kèm dữ liệu đã lọc
                .ToListAsync();
            return colors;
        }
    }
}
