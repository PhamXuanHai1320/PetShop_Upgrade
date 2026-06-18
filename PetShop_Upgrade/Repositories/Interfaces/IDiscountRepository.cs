using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IDiscountRepository : IRepository<Discount>
    {
        Task<IEnumerable<Discount>> GetAllDiscountsAsync();
        Task<Discount> GetDiscountByIdAsync(int discountId);
        Task<Discount> GetDiscountsByCodeAsync(string Code);
        Task<IEnumerable<Discount>> GetDiscountsByProductIdAsync(int productId);
        Task<IEnumerable<Discount>> GetDiscountsByCategoryIdAsync(int categoryId);
        Task<IEnumerable<Discount>> GetDiscountsByNameAsync(string discountName);
        Task<IEnumerable<Discount>> GetDiscountsByFillerAsync(DiscountFilterDTO discountFilterDTO);
    }
}
