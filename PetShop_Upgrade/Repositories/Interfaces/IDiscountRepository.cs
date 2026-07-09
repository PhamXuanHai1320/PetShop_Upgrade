using PetShop_Upgrade.DTOS.Discounts;
using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IDiscountRepository : IRepository<Discount>
    {
        Task<Discount> GetDiscountByIdAsync(int discountId);
        Task<Discount> GetDiscountsByCodeAsync(string Code);
        Task<IEnumerable<Discount>> GetDiscountsByFillerAsync(DiscountFilterDTO discountFilterDTO);
        Task<IEnumerable<Discount>> GetDiscountsByProductIdAndCategoryIdAsync(int productId, int categoryId);
        Task<IEnumerable<Discount>> GetDiscountsByProductIdAndCategoryIdAsync(IEnumerable<int> productIds, IEnumerable<int> categoryIds);
        Task<IEnumerable<Discount>> GetDiscountsByProductItemsAsync(
            IEnumerable<int> productIds, IEnumerable<int> categoryIds, decimal totalPrice);
    }
}
