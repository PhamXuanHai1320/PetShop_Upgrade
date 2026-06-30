using PetShop_Upgrade.DTOS;

namespace PetShop_Upgrade.Services.Interfaces
{
    public interface IToysDetailService
    {
        Task CreateToyDetailAsync(int productId,CreateToyDTO createToyDTO);
        Task UpdateToyDetailAsync(UpdateToyDTO updateToyDTO, int productId);
        Task<ToyResponseRequestDTO> GetToyDetailByIdAsync(int productId);
        Task<IEnumerable<ProductItemsDTO>> GetToyDetailByFillerAsync(ToyFillerDTO toyFilterDTO, int page, int pageSize);
        Task<IEnumerable<AdminToyItemDTO>> AdminGetToyDetailByFillerAsync(AdminToyFillerDTO toyFilterDTO, int page, int pageSize);
    }
}
