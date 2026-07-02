using Microsoft.EntityFrameworkCore;
using PetShop_Upgrade.Data;
using PetShop_Upgrade.DTOS.Foods.Admin;
using PetShop_Upgrade.DTOS.Foods.Client;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Repositories
{
    public class FoodDetailRepository : Repository<FoodsDetail>, IFoodDetailRepository
    {
        public FoodDetailRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<FoodsDetail> GetFoodDetailAsync(int productId)
        {
            return await _context.FoodsDetails
                .Include(fd => fd.Product)
                .Include(p => p.Product.ProductImages)
                .Include(p => p.Product.ProductColors)
                    .ThenInclude(pc => pc.Color)
                .Include(p => p.Product.Ratings)
                .FirstOrDefaultAsync(fd => fd.ProductId == productId);
        }

        public async Task<FoodsDetail> GetFoodByProductIdAsync(int productId)
        {
            return await _context.FoodsDetails
                .FirstOrDefaultAsync(fd => fd.ProductId == productId);
        }

        public async Task<IEnumerable<FoodsDetail>> GetFoodByFillerAsync(
            FoodFilterDTO foodFilterDTO, int page, int pageSize)
        {
            var query = _context.FoodsDetails
                .Include(p => p.Product)
                .Include(p => p.Product.ProductImages)
                .Include(p => p.Product.ProductColors)
                    .ThenInclude(pc => pc.Color)
                .Include(p => p.Product.Ratings)
                .Include(p => p.Product.Category)
                .AsQueryable()
                .Where(p => p.Product.IsActive == IsActive.ACTIVE && p.Product.Type == ProductType.Food);

            if (!string.IsNullOrEmpty(foodFilterDTO.ProductName))
                query = query.Where(
                    p => EF.Functions.Collate(p.Product.ProductName, "SQL_Latin1_General_CP1_CI_AI").Contains(foodFilterDTO.ProductName));
            if (foodFilterDTO.CategoryId.HasValue)
                query = query.Where(p => p.Product.CategoryId == foodFilterDTO.CategoryId.Value);
            if (foodFilterDTO.MinPrice.HasValue)
                query = query.Where(p => p.Product.SellingPrice >= foodFilterDTO.MinPrice.Value);
            if (foodFilterDTO.MaxPrice.HasValue)
                query = query.Where(p => p.Product.SellingPrice <= foodFilterDTO.MaxPrice.Value);
            if (foodFilterDTO.ColorId.HasValue)
                query = query.Where(p => p.Product.ProductColors.Any(pc => pc.ColorId == foodFilterDTO.ColorId));
            if (!String.IsNullOrEmpty(foodFilterDTO.Flavor))
                query = query.Where(p => p.Flavor == foodFilterDTO.Flavor);
            if (!String.IsNullOrEmpty(foodFilterDTO.AgeGroup))
                query = query.Where(p => p.AgeGroup == foodFilterDTO.AgeGroup);
            if (foodFilterDTO.MinWeightGram.HasValue && foodFilterDTO.MaxWeightGram.HasValue)
                query = query.Where(p => p.WeightGram >= foodFilterDTO.MinWeightGram && p.WeightGram <= foodFilterDTO.MaxWeightGram);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<FoodsDetail>> AdminGetFoodDetailByFillerAsync(
            AdminFoodFilterDTO foodFilterDTO, int page, int pageSize)
        {
            var query = _context.FoodsDetails
                .Include(p => p.Product)
                .Include(p => p.Product.ProductImages)
                .Include(p => p.Product.Category)
                .AsQueryable()
                .Where(p => p.Product.Type == ProductType.Food);

            if (foodFilterDTO.ProductId.HasValue)
                query = query.Where(p => p.ProductId == foodFilterDTO.ProductId.Value);
            if (!string.IsNullOrEmpty(foodFilterDTO.ProductName))
                query = query.Where(
                    p => EF.Functions
                    .Collate(p.Product.ProductName, "SQL_Latin1_General_CP1_CI_AI").Contains(foodFilterDTO.ProductName));
            if (foodFilterDTO.CategoryId.HasValue)
                query = query.Where(p => p.Product.CategoryId == foodFilterDTO.CategoryId.Value);
            if (foodFilterDTO.ImportPrice.HasValue)
                query = query.Where(p => p.Product.ImportPrice == foodFilterDTO.ImportPrice.Value);
            if (foodFilterDTO.SellingPrice.HasValue)
                query = query.Where(p => p.Product.SellingPrice == foodFilterDTO.SellingPrice.Value);
            if(foodFilterDTO.ExpiryStatus != null)
            {
                if (foodFilterDTO.ExpiryStatus == ExpiryStatus.EXPIRED)
                {
                    query = query.Where(p => p.ExprireDate < DateTime.Now);
                }
                else if (foodFilterDTO.ExpiryStatus == ExpiryStatus.EXPIREDSOON) // Not expired
                {
                    DateTime warningDate = DateTime.Today.AddDays(30);
                    query = query.Where(p => p.ExprireDate >= DateTime.Today
                                          && p.ExprireDate <= DateTime.Today.AddDays(30));
                }
            }

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
