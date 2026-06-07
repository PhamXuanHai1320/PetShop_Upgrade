using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IMemberRepository : IRepository<Member>
    {
        Task<Member> GetMemberByEmailAsync(string email);
        Task<bool> CheckDuplicateMember(string email, string username, int id);
        Task<bool> CheckDuplicateMember(string email, string username);
    }
}
