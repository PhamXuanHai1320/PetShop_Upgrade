using Microsoft.EntityFrameworkCore;
using PetShop_Upgrade.Data;
using PetShop_Upgrade.Migrations;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;

namespace PetShop_Upgrade.Repositories
{
    public class ProductColorRepository : Repository<ProductColor>, IProductColorRepository
    {
        public ProductColorRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<ProductColor?> GetForUpdateAsync(int productColorId)
        {
            // UPDLOCK: khóa ngay lúc đọc, transaction khác phải chờ tới khi commit/rollback
            // ROWLOCK: chỉ khóa đúng 1 row, không ảnh hưởng các row khác trong bảng
            var result = await _context.ProductColors
                .FromSqlInterpolated($@"
                    SELECT * FROM ProductColors WITH (UPDLOCK, ROWLOCK)
                    WHERE Id = {productColorId}")
                .AsTracking()
                .FirstOrDefaultAsync();

            return result;
        }
        public async Task<List<ProductColor>> GetForUpdateBatchAsync(IEnumerable<int> productColorIds)
        {
            var sortedIds = productColorIds.OrderBy(id => id).ToList();
            if (sortedIds.Count == 0) return new List<ProductColor>();

            var idsCsv = string.Join(",", sortedIds);

            var sql = $@"
                SELECT * FROM ProductColors WITH (UPDLOCK, ROWLOCK)
                WHERE Id IN ({idsCsv})
                ORDER BY Id";

            return await _context.ProductColors
                .FromSqlRaw(sql)
                .ToListAsync();
        }
    }
}
