using Microsoft.EntityFrameworkCore;
using Minio.DataModel.Notification;
using PetShop_Upgrade.Data;
using PetShop_Upgrade.DTOS.Products.Client;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<Product> GetProductByIdAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductColors)
                    .ThenInclude(pc => pc.Color)
                .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive == IsActive.ACTIVE);
        }
        public async Task<Product> GetProductDetailsByIdAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductColors)
                    .ThenInclude(pc => pc.Color)
                .Include(p => p.Category)
                .Include(p => p.Ratings)
                .Include(p => p.ToysDetails)
                .Include(p => p.FoodsDetails)
                .Include(p => p.PetVariant)
                .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive == IsActive.ACTIVE);
        }
        public async Task<bool> HasProductsByCategoryIdAsync(int categoryId)
        {
            return await _context.Products
                .AnyAsync(p => p.CategoryId == categoryId);
        }
        public async Task<IEnumerable<Product>> GetProductByFilterAsync(
            ProductFilterDTO productFillerDTO, int page, int pageSize)
        {
            var query = GetBaseProductQuery();
            query = ApplyBaseFilter(query, productFillerDTO, checkActive: true);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        // Base filter dùng chung cho tất cả loại
        private IQueryable<Product> ApplyBaseFilter(
            IQueryable<Product> query, ProductFilterDTO filter, bool checkActive)
        {
            if (checkActive)
                query = query.Where(p => p.IsActive == IsActive.ACTIVE);

            if (!string.IsNullOrEmpty(filter.ProductName))
                query = query.Where(
                    p => EF.Functions.Collate(p.ProductName, "SQL_Latin1_General_CP1_CI_AI").Contains(filter.ProductName));
            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.SellingPrice >= filter.MinPrice.Value);
            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.SellingPrice <= filter.MaxPrice.Value);
            if (filter.Type.HasValue)
                query = query.Where(p => p.Type == filter.Type.Value);
            if (filter.ColorId.HasValue)
                query = query.Where(p => p.ProductColors.Any(pc => pc.ColorId == filter.ColorId));
            return query;
        }
        // Query chung cho tất cả hàm
        private IQueryable<Product> GetBaseProductQuery()
        {
            return _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.Ratings);
        }
    }
}
