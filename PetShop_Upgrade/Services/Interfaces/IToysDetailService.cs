using PetShop_Upgrade.DTOS.Products.Client;
using PetShop_Upgrade.DTOS.Toys.Admin;
using PetShop_Upgrade.DTOS.Toys.Client;

namespace PetShop_Upgrade.Services.Interfaces
{
    public interface IToysDetailService
    {
        Task CreateToyDetailAsync(int productId,CreateToyDTO createToyDTO);
        Task UpdateToyDetailAsync(UpdateToyDTO updateToyDTO, int productId);
        Task<ToyResponseRequestDTO> GetToyDetailByIdAsync(int productId);
        Task<IEnumerable<ProductItemsDTO>> GetToyDetailByFillerAsync(ToyFilterDTO toyFilterDTO, int page, int pageSize);
        Task<IEnumerable<AdminToyItemDTO>> AdminGetToyDetailByFillerAsync(AdminToyFilterDTO toyFilterDTO, int page, int pageSize);
    }
}
