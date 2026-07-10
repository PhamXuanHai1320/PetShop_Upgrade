using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Services
{
    public class InventoryLockExpirationWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<InventoryLockExpirationWorker> _logger;

        public InventoryLockExpirationWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<InventoryLockExpirationWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                    await orderService.ExpirePendingVNPayOrdersAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi xử lý InventoryLock hết hạn");
                }

                await timer.WaitForNextTickAsync(stoppingToken);
            }
        }
    }
}
