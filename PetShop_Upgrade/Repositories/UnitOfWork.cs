using PetShop_Upgrade.Data;
using PetShop_Upgrade.Repositories.Interfaces;

namespace PetShop_Upgrade.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            MemberRepository = new MemberRepository(_context);
            RefreshTokenRepository = new RefreshTokenRepository(_context);
            CartRepository = new CartRepository(_context);
        }

        public IMemberRepository MemberRepository { get; private set; }

        public IRefreshTokenRepository RefreshTokenRepository { get; private set; }

        public ICartRepository CartRepository { get; private set; }

        public void Dispose()
        {
            _context.Dispose();
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
