namespace PetShop_Upgrade.Services.Interfaces
{
    public interface IMemberService
    {
        Task BlockMemberAsync(string memberId);
    }
}
