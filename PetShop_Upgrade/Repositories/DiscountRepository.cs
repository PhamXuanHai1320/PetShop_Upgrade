using Microsoft.EntityFrameworkCore;
using PetShop_Upgrade.Data;
using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;

namespace PetShop_Upgrade.Repositories
{
    public class DiscountRepository : Repository<Discount>, IDiscountRepository
    {
        public DiscountRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Discount> GetDiscountByIdAsync(int discountId)
        {
            return await _context.Discounts
                .FirstOrDefaultAsync(d => d.Id == discountId);
        }

        public async Task<IEnumerable<Discount>> GetDiscountsByCategoryIdAsync(int categoryId)
        {
            return await _context.Discounts
                .Where(d => d.DiscountCategories.Any(dc => dc.CategoryId == categoryId))
                .OrderByDescending(d => d.CreateAt)
                .ToListAsync();
        }

        public async Task<Discount> GetDiscountsByCodeAsync(string code)
        {
            return await _context.Discounts
                .FirstOrDefaultAsync(d => d.Code == code);
        }

        public async Task<IEnumerable<Discount>> GetDiscountsByFillerAsync(DiscountFilterDTO discountFilterDTO)
        {
            var query = _context.Discounts.AsQueryable();

            if (!string.IsNullOrEmpty(discountFilterDTO.Keyword))
                query = query.Where(d =>
                    d.Code.Contains(discountFilterDTO.Keyword) ||
                    d.DiscountName.Contains(discountFilterDTO.Keyword));

            if (discountFilterDTO.DiscountType.HasValue)
                query = query.Where(d => d.DiscountType == discountFilterDTO.DiscountType);

            if (discountFilterDTO.IsActive.HasValue)
                query = query.Where(d => d.IsActive == discountFilterDTO.IsActive);

            if (discountFilterDTO.StartDateFrom.HasValue)
                query = query.Where(d => d.StartDate >= discountFilterDTO.StartDateFrom);

            if (discountFilterDTO.StartDateTo.HasValue)
                query = query.Where(d => d.StartDate <= discountFilterDTO.StartDateTo);

            if (discountFilterDTO.EndDateFrom.HasValue)
                query = query.Where(d => d.EndDate >= discountFilterDTO.EndDateFrom);

            if (discountFilterDTO.EndDateTo.HasValue)
                query = query.Where(d => d.EndDate <= discountFilterDTO.EndDateTo);

            return await query
                .OrderByDescending(d => d.CreateAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Discount>> GetDiscountsByNameAsync(string discountName)
        {
            return await _context.Discounts
                .Where(d => d.DiscountName.Contains(discountName))
                .OrderByDescending(d => d.CreateAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Discount>> GetDiscountsByProductIdAsync(int productId)
        {
            return await _context.Discounts
                .Where(d => d.DiscountProducts.Any(dp => dp.ProductId == productId))
                .OrderByDescending(d => d.CreateAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Discount>> GetAllDiscountsAsync()
        {
            return await _context.Discounts
                .Where(
                    d => d.IsActive == 1 &&
                    d.StartDate <= DateTime.UtcNow &&
                    (d.EndDate == null || d.EndDate >= DateTime.UtcNow) &&
                    (d.MaxUsage == null || d.DiscountUsages.Count < d.MaxUsage))
                .OrderByDescending(d => d.CreateAt)
                .ToListAsync();
        }
    }
}
