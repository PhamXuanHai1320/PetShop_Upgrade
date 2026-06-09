namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        void Dispose();
        Task SaveChangesAsync();
        IMemberRepository MemberRepository { get; }
        IRefreshTokenRepository RefreshTokenRepository { get; }
        ICartRepository CartRepository { get; }
        IColorRepository ColorRepository { get; }
    }
}
