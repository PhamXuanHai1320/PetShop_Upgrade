using PetShop_Upgrade.DTOS.Pets.Admin;
using PetShop_Upgrade.DTOS.Pets.Client;
using PetShop_Upgrade.DTOS.Products.Client;

namespace PetShop_Upgrade.Services.Interfaces
{
    public interface IPetVariantService
    {
        Task CreatePetVariantAsync(int productId, CreatePetDTO createPetDTO);
        Task UpdatePetVariantAsync(UpdatePetDTO updatePetDTO, int productId);
        Task<PetResponseRequestDTO> GetPetVariantByIdAsync(int productId);
        Task<IEnumerable<ProductItemsDTO>> GetPetVariantByFillerAsync(PetFilterDTO petFilterDTO, int page, int pageSize);
        Task<IEnumerable<AdminPetVariantItemDTO>> AdminGetPetVariantByFillerAsync(AdminPetFillerDTO petFilterDTO, int page, int pageSize);
    }
}
