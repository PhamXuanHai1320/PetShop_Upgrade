using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IFoodDetailRepository : IRepository<FoodsDetail>
    {
        Task<FoodsDetail> GetFoodDetailAsync(int productId);
        Task<FoodsDetail> GetFoodByProductIdAsync(int productId);
        Task<IEnumerable<FoodsDetail>> GetFoodByFillerAsync(FoodFillerDTO foodFilterDTO, int page, int pageSize);
        Task<IEnumerable<FoodsDetail>> AdminGetFoodDetailByFillerAsync(AdminFoodFillerDTO foodFilterDTO, int page, int pageSize);
    }
}
