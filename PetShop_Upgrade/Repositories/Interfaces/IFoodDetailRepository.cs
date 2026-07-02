using PetShop_Upgrade.DTOS.Foods.Admin;
using PetShop_Upgrade.DTOS.Foods.Client;
using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IFoodDetailRepository : IRepository<FoodsDetail>
    {
        Task<FoodsDetail> GetFoodDetailAsync(int productId);
        Task<FoodsDetail> GetFoodByProductIdAsync(int productId);
        Task<IEnumerable<FoodsDetail>> GetFoodByFillerAsync(FoodFilterDTO foodFilterDTO, int page, int pageSize);
        Task<IEnumerable<FoodsDetail>> AdminGetFoodDetailByFillerAsync(AdminFoodFilterDTO foodFilterDTO, int page, int pageSize);
    }
}
