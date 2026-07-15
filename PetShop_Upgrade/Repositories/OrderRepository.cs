using Microsoft.EntityFrameworkCore;
using PetShop_Upgrade.Data;
using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.DTOS.Order.Admin;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<Order?> GetOrderWithDetailsByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.InventoryLocks)
                .Include(o => o.DiscountUsages)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }
        public async Task<Order?> GetOrderForUpdateAsync(int orderId)
        {
            return await _context.Orders
                .FromSqlInterpolated($@"SELECT * FROM Orders WITH (UPDLOCK, ROWLOCK) WHERE Id = {orderId}")
                .Include(o => o.OrderDetails)
                .Include(o => o.InventoryLocks)
                .Include(o => o.DiscountUsages)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync();
        }

        public async Task<PagedData<Order>> AdminGetOrdersByFilterAsync(
            AdminOrderFilterDTO orderFilterDTO, int page, int pageSize)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Orders
                .AsNoTracking()
            .AsQueryable();

            if (orderFilterDTO.Status.HasValue)
            {
                query = query.Where(o => o.status == orderFilterDTO.Status.Value);
            }

            if (orderFilterDTO.DateFrom.HasValue)
            {
                query = query.Where(o =>
                    o.CreatedAt >= orderFilterDTO.DateFrom.Value);
            }

            if (orderFilterDTO.DateTo.HasValue)
            {
                var dateToExclusive = orderFilterDTO.DateTo.Value.Date.AddDays(1);

                query = query.Where(o =>
                    o.CreatedAt < dateToExclusive);
            }

            if (orderFilterDTO.PaymentStatus.HasValue)
            {
                query = query.Where(o =>
                o.Payment != null &&
                    o.Payment.PaymentStatus == orderFilterDTO.PaymentStatus.Value);
            }
            var totalItems = await query.CountAsync();
            var orders = await query
                .AsNoTracking()
                .Include(o => o.OrderDetails)
                .Include(o => o.Payment)
                .Include(o => o.Member)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new PagedData<Order>
            {
                Items = orders,
                TotalItems = totalItems
            };
        }

        public async Task<PagedData<Order>> GetOrdersByOrderStatusAsync(
            OrderStatus status, int memberId, int page, int pageSize)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Orders
                .AsNoTracking()
                .Where(o =>
                    o.status == status &&
                    o.MemberId == memberId);
            var totalItems = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Include(o => o.OrderDetails)
                .Include(o => o.Payment)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new PagedData<Order>
            {
                Items = orders,
                TotalItems = totalItems
            };
        }

        public async Task<Order?> AdminGetOrderDetail(int orderId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.ProductImages)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(pc => pc.ProductColors)
                            .ThenInclude(pc => pc.Color)
                .Include(o => o.Payment)
                .Include(o => o.Member)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<Order?> GetOrderDetail(int orderId, int memberId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.ProductImages)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(pc => pc.ProductColors)
                            .ThenInclude(pc => pc.Color)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId &&
                    o.MemberId == memberId &&
                    o.status != OrderStatus.EXPIRED);
        }
    }
}
