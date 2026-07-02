using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using PetShop_Upgrade.Data;
using PetShop_Upgrade.DTOS.Toys.Admin;
using PetShop_Upgrade.DTOS.Toys.Client;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Repositories
{
    public class ToyDetailRepository : Repository<ToysDetail>, IToyDetailRepository
    {
        public ToyDetailRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<ToysDetail> GetToyDetailAsync(int productId)
        {
            return await _context.ToysDetails
                .Include(t => t.Product)
                .Include(p => p.Product.ProductImages)
                .Include(p => p.Product.ProductColors)
                    .ThenInclude(pc => pc.Color)
                .Include(p => p.Product.Ratings)
                .FirstOrDefaultAsync(t => t.ProductId == productId);
        }

        public async Task<ToysDetail> GetToyByProductIdAsync(int productId)
        {
            return await _context.ToysDetails
                .FirstOrDefaultAsync(t => t.ProductId == productId);
        }

        public async Task<IEnumerable<ToysDetail>> GetToyByFillerAsync(
            ToyFilterDTO toyFilterDTO, int page, int pageSize)
        {
            var query = _context.ToysDetails
                .Include(p => p.Product)
                .Include(p => p.Product.ProductImages)
                .Include(p => p.Product.ProductColors)
                    .ThenInclude(pc => pc.Color)
                .Include(p => p.Product.Ratings)
                .Include(p => p.Product.Category)
                .AsQueryable()
                .Where(p => p.Product.IsActive == IsActive.ACTIVE && p.Product.Type == ProductType.Toy);

            if (!string.IsNullOrEmpty(toyFilterDTO.ProductName))
                query = query.Where(
                    p => EF.Functions.Collate(p.Product.ProductName, "SQL_Latin1_General_CP1_CI_AI").Contains(toyFilterDTO.ProductName));
            if (toyFilterDTO.CategoryId.HasValue)
                query = query.Where(p => p.Product.CategoryId == toyFilterDTO.CategoryId.Value);
            if (toyFilterDTO.MinPrice.HasValue)
                query = query.Where(p => p.Product.SellingPrice >= toyFilterDTO.MinPrice.Value);
            if (toyFilterDTO.MaxPrice.HasValue)
                query = query.Where(p => p.Product.SellingPrice <= toyFilterDTO.MaxPrice.Value);
            if (toyFilterDTO.ColorId.HasValue)
                query = query.Where(p => p.Product.ProductColors.Any(pc => pc.ColorId == toyFilterDTO.ColorId));
            if (!String.IsNullOrEmpty(toyFilterDTO.Material))
                query = query.Where(p => p.Material == toyFilterDTO.Material);
            if (!String.IsNullOrEmpty(toyFilterDTO.Size))
                query = query.Where(p => p.Size == toyFilterDTO.Size);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<ToysDetail>> AdminGetToyDetailByFillerAsync(
            AdminToyFilterDTO toyFilterDTO, int page, int pageSize)
        {
            var query = _context.ToysDetails
                .Include(p => p.Product)
                .Include(p => p.Product.ProductImages)
                .Include(p => p.Product.Category)
                .AsQueryable()
                .Where(p => p.Product.Type == ProductType.Toy);

            if (toyFilterDTO.ProductId.HasValue)
                query = query.Where(p => p.ProductId == toyFilterDTO.ProductId.Value);
            if (!string.IsNullOrEmpty(toyFilterDTO.ProductName))
                query = query.Where(
                    p => EF.Functions
                    .Collate(p.Product.ProductName, "SQL_Latin1_General_CP1_CI_AI").Contains(toyFilterDTO.ProductName));
            if (toyFilterDTO.CategoryId.HasValue)
                query = query.Where(p => p.Product.CategoryId == toyFilterDTO.CategoryId.Value);
            if (toyFilterDTO.ImportPrice.HasValue)
                query = query.Where(p => p.Product.ImportPrice == toyFilterDTO.ImportPrice.Value);
            if (toyFilterDTO.SellingPrice.HasValue)
                query = query.Where(p => p.Product.SellingPrice == toyFilterDTO.SellingPrice.Value);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
