using PetShop_Upgrade.DTOS.Toys.Admin;
using PetShop_Upgrade.DTOS.Toys.Client;
using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IToyDetailRepository : IRepository<ToysDetail>
    {
        Task<ToysDetail> GetToyDetailAsync(int productId);
        Task<ToysDetail> GetToyByProductIdAsync(int productId);
        Task<IEnumerable<ToysDetail>> GetToyByFillerAsync(ToyFilterDTO toyFilterDTO, int page, int pageSize);
        Task<IEnumerable<ToysDetail>> AdminGetToyDetailByFillerAsync(AdminToyFilterDTO toyFilterDTO, int page, int pageSize);
    }
}
