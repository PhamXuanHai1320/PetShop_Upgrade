using PetShop_Upgrade.DTOS;

namespace PetShop_Upgrade.Services.Interfaces
{
    public interface IPetVariantService
    {
        Task CreatePetVariantAsync(int productId, CreatePetDTO createPetDTO);
        Task UpdatePetVariantAsync(UpdatePetDTO updatePetDTO, int productId);
        Task<PetResponseRequestDTO> GetPetVariantByIdAsync(int productId);
        Task<IEnumerable<ProductItemsDTO>> GetPetVariantByFillerAsync(PetFillerDTO petFilterDTO, int page, int pageSize);
        Task<IEnumerable<AdminPetVariantItemDTO>> AdminGetPetVariantByFillerAsync(AdminPetFillerDTO petFilterDTO, int page, int pageSize);
    }
}
