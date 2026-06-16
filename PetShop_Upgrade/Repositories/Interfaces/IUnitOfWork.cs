using Microsoft.EntityFrameworkCore.Storage;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        IMemberRepository MemberRepository { get; }
        IRefreshTokenRepository RefreshTokenRepository { get; }
        ICartRepository CartRepository { get; }
        IColorRepository ColorRepository { get; }
        ICategoryRepository CategoryRepository  { get; }
    }
}
