using PetShop_Upgrade.DTOS;

namespace PetShop_Upgrade.Services.Interfaces
{
    public interface IFoodsDetailService
    {
        Task CreateFoodDetailAsync(int productId, CreateFoodDTO createFoodDTO);
        Task UpdateFoodDetailAsync(UpdateFoodDTO updateFoodDTO, int productId);
        Task<FoodResponseRequestDTO> GetFoodDetailByIdAsync(int productId);
        Task<IEnumerable<ProductItemsDTO>> GetFoodDetailByFillerAsync(FoodFillerDTO foodFilterDTO, int page, int pageSize);
        Task<IEnumerable<AdminFoodItemDTO>> AdminGetFoodDetailByFillerAsync(AdminFoodFillerDTO foodFilterDTO, int page, int pageSize);
    }
}
