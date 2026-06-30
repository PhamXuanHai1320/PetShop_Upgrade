using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IToyDetailRepository : IRepository<ToysDetail>
    {
        Task<ToysDetail> GetToyDetailAsync(int productId);
        Task<ToysDetail> GetToyByProductIdAsync(int productId);
        Task<IEnumerable<ToysDetail>> GetToyByFillerAsync(ToyFillerDTO toyFilterDTO, int page, int pageSize);
        Task<IEnumerable<ToysDetail>> AdminGetToyDetailByFillerAsync(AdminToyFillerDTO toyFilterDTO, int page, int pageSize);
    }
}
