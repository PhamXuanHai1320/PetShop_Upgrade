using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IPetVariantRepository : IRepository<PetVariant>
    {
        Task<PetVariant> GetPetVariantAsync(int productId);
        Task<PetVariant> GetPetvariantByProductIdAsync(int productId);
        Task<IEnumerable<PetVariant>> GetPetVariantByFillerAsync(PetFillerDTO petFilterDTO, int page, int pageSize);
        Task<IEnumerable<PetVariant>> AdminGetPetVariantByFillerAsync(AdminPetFillerDTO petFilterDTO, int page, int pageSize);
    }
}
