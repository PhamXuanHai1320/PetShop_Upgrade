using Microsoft.EntityFrameworkCore;
using Minio.DataModel.Notification;
using PetShop_Upgrade.Data;
using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Repositories
{
    public class PetVariantRepository : Repository<PetVariant>, IPetVariantRepository
    {
        public PetVariantRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PetVariant> GetPetVariantAsync(int productId)
        {
            return await _context.PetVariant
                .Include(p => p.Product)
                .Include(p => p.Product.ProductImages)
                .Include(p => p.Product.ProductColors)
                    .ThenInclude(pc => pc.Color)
                .Include(p => p.Product.Ratings)
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }
        public async Task<PetVariant> GetPetvariantByProductIdAsync(int productId)
        {
            return await _context.PetVariant
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public async Task<IEnumerable<PetVariant>> GetPetVariantByFillerAsync(
            PetFillerDTO petFilterDTO, int page, int pageSize)
        {
            var query = _context.PetVariant
                .Include(p => p.Product)
                .Include(p => p.Product.ProductImages)
                .Include(p => p.Product.Ratings)
                .Include(p => p.Product.Category)
                .AsQueryable()
                .Where(p => p.Product.IsActive == IsActive.ACTIVE && p.Product.Type == ProductType.Pet);

            if (!string.IsNullOrEmpty(petFilterDTO.ProductName))
                query = query.Where(
                    p => EF.Functions.Collate(p.Product.ProductName, "SQL_Latin1_General_CP1_CI_AI").Contains(petFilterDTO.ProductName));
            if (petFilterDTO.CategoryId.HasValue)
                query = query.Where(p => p.Product.CategoryId == petFilterDTO.CategoryId.Value);
            if (petFilterDTO.MinPrice.HasValue)
                query = query.Where(p => p.Product.SellingPrice >= petFilterDTO.MinPrice.Value);
            if (petFilterDTO.MaxPrice.HasValue)
                query = query.Where(p => p.Product.SellingPrice <= petFilterDTO.MaxPrice.Value);
            if (petFilterDTO.ColorId.HasValue)
                query = query.Where(p => p.Product.ProductColors.Any(pc => pc.ColorId == petFilterDTO.ColorId));
            if(!String.IsNullOrEmpty(petFilterDTO.Gender))
                query = query.Where(p => p.Gender == petFilterDTO.Gender);
            if (!String.IsNullOrEmpty(petFilterDTO.Size))
                query = query.Where(p => p.Size == petFilterDTO.Size);
            if (petFilterDTO.MinWeight.HasValue && petFilterDTO.MaxWeight.HasValue)
                query = query.Where(p => p.Weight >= petFilterDTO.MinWeight && p.Weight <= petFilterDTO.MaxWeight);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<PetVariant>> AdminGetPetVariantByFillerAsync(
            AdminPetFillerDTO petFilterDTO, int page, int pageSize)
        {
            var query = _context.PetVariant
                .Include(p => p.Product)
                .Include(p => p.Product.ProductImages)
                .Include(p => p.Product.Category)
                .AsQueryable()
                .Where(p => p.Product.Type == ProductType.Pet);

            if (petFilterDTO.ProductId.HasValue)
                query = query.Where(p => p.ProductId == petFilterDTO.ProductId.Value);
            if (!string.IsNullOrEmpty(petFilterDTO.ProductName))
                query = query.Where(
                    p => EF.Functions.Collate(p.Product.ProductName, "SQL_Latin1_General_CP1_CI_AI").Contains(petFilterDTO.ProductName));
            if (petFilterDTO.CategoryId.HasValue)
                query = query.Where(p => p.Product.CategoryId == petFilterDTO.CategoryId.Value);
            if (petFilterDTO.ImportPrice.HasValue)
                query = query.Where(p => p.Product.ImportPrice == petFilterDTO.ImportPrice.Value);
            if (petFilterDTO.SellingPrice.HasValue)
                query = query.Where(p => p.Product.SellingPrice == petFilterDTO.SellingPrice.Value);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
