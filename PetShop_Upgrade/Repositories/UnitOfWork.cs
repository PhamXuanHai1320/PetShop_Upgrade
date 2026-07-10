using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PetShop_Upgrade.Data;
using PetShop_Upgrade.Models;
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
            ColorRepository = new ColorRepository(_context);
            CategoryRepository = new CategoryRepository(_context);
            ProductRepository = new ProductRepository(_context);
            PetVariantRepository = new PetVariantRepository(_context);
            FoodDetailRepository = new FoodDetailRepository(_context);
            ToyDetailRepository = new ToyDetailRepository(_context);
            ProductHistoryRepository = new ProductHistoryRepository(_context);
            DiscountRepository = new DiscountRepository(_context);
            CartItemRepository = new Repository<CartItem>(context);
            ProductColorRepository = new ProductColorRepository(context);
            InventoryLockRepository = new InventoryLockRepository(context);
            OrderDetailRepository = new Repository<OrderDetail>(context);
            OrderRepository = new OrderRepository(context);
            AddressRepository = new AddressRepository(context);
            DiscountUsageRepository = new Repository<DiscountUsage>(context);
            AppointmentRepository = new AppointmentRepository(context);
            PetViewingAppointmentRepository = new Repository<PetViewingAppointment>(context);
            PaymentWebhookLogRepository = new Repository<PaymentWebhookLog>(context);
        }

        public IMemberRepository MemberRepository { get; private set; }

        public IRefreshTokenRepository RefreshTokenRepository { get; private set; }

        public ICartRepository CartRepository { get; private set; }
        public IColorRepository ColorRepository { get; private set; }
        public ICategoryRepository CategoryRepository { get; private set; }
        public IProductRepository ProductRepository { get; private set; }
        public IProductHistoryRepository ProductHistoryRepository { get; private set; }

        public IDiscountRepository DiscountRepository { get; private set; }

        public IRepository<CartItem> CartItemRepository { get; private set; }

        public IPetVariantRepository PetVariantRepository { get; private set; }

        public IFoodDetailRepository FoodDetailRepository { get; private set; }

        public IToyDetailRepository ToyDetailRepository { get; private set; }

        public IProductColorRepository ProductColorRepository { get; private set; }

        public Interfaces.IInventoryLockRepository InventoryLockRepository { get; private set; }

        public IRepository<OrderDetail> OrderDetailRepository { get; private set; }

        public IOrderRepository OrderRepository { get; private set; }

        public IAddressRepository AddressRepository { get; private set; }

        public IRepository<DiscountUsage> DiscountUsageRepository { get; private set; }

        public IAppointmentRepository AppointmentRepository { get; private set; }

        public IRepository<PetViewingAppointment> PetViewingAppointmentRepository { get; private set; }

        public IRepository<PaymentWebhookLog> PaymentWebhookLogRepository { get; private set; }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        }
    }
}
