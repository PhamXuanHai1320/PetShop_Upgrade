using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using PetShop_Upgrade.Services.Interfaces;
using PetShop_Upgrade.DTOS.VNPay;
using static PetShop_Upgrade.Models.Enum;
using System.Text.Json;

namespace PetShop_Upgrade.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentService> _logger;
        private readonly string _vnpTmnCode;

        public PaymentService(IUnitOfWork unitOfWork, ILogger<PaymentService> logger, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _vnpTmnCode = configuration["VNPay:TmnCode"] ?? string.Empty;
        }

        // Ghi log mọi webhook nhận được, dù thành công hay thất bại - phục vụ audit sau này
        public async Task LogWebhookAsync(string orderCode, string rawPayload, string signature, bool isVerified)
        {
            var log = new PaymentWebhookLog
            {
                OrderCode = orderCode,
                RawPayload = rawPayload,
                Signature = signature,
                IsVerified = isVerified ? 1 : 0,
                ProcessedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.PaymentWebhookLogRepository.Add(log);
            await _unitOfWork.SaveChangesAsync();
        }
        // Ghi nhận thanh toán thành công, cập nhật trạng thái Order và Payment
        public async Task<bool> MarkPaymentSuccessAsync(int orderId, string transactionNo)
        {
            var order = await _unitOfWork.OrderRepository.GetOrderWithDetailsByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Nhận webhook thành công nhưng không tìm thấy Order, OrderId = {OrderId}", orderId);
                return false;
            }

            if (order.status != OrderStatus.PENDING)
            {
                _logger.LogInformation(
                    "Bỏ qua webhook trùng lặp, OrderId = {OrderId} đã ở trạng thái {Status}", orderId, order.status);
                return false;
            }

            if (order.Payment == null)
            {
                _logger.LogWarning("OrderId = {OrderId} không có Payment record để cập nhật", orderId);
                return false;
            }

            order.Payment.PaymentStatus = PaymentStatus.PAID;
            order.Payment.TransactionId = transactionNo;
            order.Payment.PaidAt = DateTime.UtcNow;

            _unitOfWork.OrderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Cập nhật Payment thành công, OrderId = {OrderId}, TransactionNo = {TransactionNo}",
                orderId, transactionNo);

            return true;
        }
        // Ghi nhận thanh toán thất bại, cập nhật trạng thái Order và Payment
        public async Task<bool> MarkPaymentFailedAsync(int orderId, string responseCode)
        {
            var order = await _unitOfWork.OrderRepository.GetOrderWithDetailsByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Nhận webhook thất bại nhưng không tìm thấy Order, OrderId = {OrderId}", orderId);
                return false;
            }

            if (order.status != OrderStatus.PENDING)
            {
                _logger.LogInformation(
                    "Bỏ qua webhook thất bại trùng lặp, OrderId = {OrderId} đã ở trạng thái {Status}", orderId, order.status);
                return false;
            }

            if (order.Payment != null)
            {
                order.Payment.PaymentStatus = PaymentStatus.FAILED;
                _unitOfWork.OrderRepository.Update(order);
                await _unitOfWork.SaveChangesAsync();
            }

            _logger.LogInformation("Cập nhật Payment thất bại, OrderId = {OrderId}, ResponseCode = {ResponseCode}",
                orderId, responseCode);

            return true;
        }
        // Xử lý callback từ VNPay, xác thực chữ ký, cập nhật trạng thái Order và Payment
        public async Task<VNPayIpnResponseDTO> ProcessVNPayIpnAsync(
            VNPayCallbackResultDTO callback, string signature)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.PaymentWebhookLogRepository.Add(new PaymentWebhookLog
                {
                    OrderCode = callback.TxnRef,
                    RawPayload = callback.RawQuery,
                    Signature = signature,
                    IsVerified = callback.IsSignatureValid ? 1 : 0,
                    ProcessedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });
                // Ghi log webhook trước khi xác thực chữ ký và cập nhật trạng thái Order/Payment
                if (!callback.IsSignatureValid ||
                    !string.Equals(callback.TmnCode, _vnpTmnCode, StringComparison.Ordinal))
                {
                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new("97", "Invalid signature or terminal code");
                }
                // Nếu chữ ký hợp lệ, tiến hành xác thực Order và cập nhật trạng thái Payment
                var order = await _unitOfWork.OrderRepository.GetOrderForUpdateAsync(callback.OrderId);
                if (order?.Payment == null)
                {
                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new("01", "Order not found");
                }
                // Nếu phương thức thanh toán không phải VNPay, bỏ qua callback
                if (order.Payment.PaymentMethod != PaymentMethod.VNPAY)
                {
                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new("02", "Order already confirmed");
                }
                // Nếu số tiền thanh toán không khớp, bỏ qua callback
                if (order.Payment.TotalPrice != callback.Amount)
                {
                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new("04", "Invalid amount");
                }
                // Nếu trạng thái Order hoặc Payment không phải PENDING, bỏ qua callback
                if (order.status != OrderStatus.PENDING || order.Payment.PaymentStatus != PaymentStatus.PENDING)
                {
                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new("02", "Order already updated");
                }
                // Nếu callback thành công, cập nhật trạng thái Order và Payment, đồng thời xác nhận các InventoryLock
                if (callback.ResponseCode == "00" && callback.TransactionStatus == "00")
                {
                    order.Payment.PaymentStatus = PaymentStatus.PAID;
                    order.Payment.TransactionId = callback.TransactionNo;
                    order.Payment.PaidAt = DateTime.UtcNow;
                    order.status = OrderStatus.CONFIRMED;
                    foreach (var inventoryLock in order.InventoryLocks.Where(x => x.Status == InventoryLockStatus.LOCKED))
                        inventoryLock.Status = InventoryLockStatus.CONFIRMED;

                    AddOutboxMessage("payment.succeeded", order, callback);
                }
                else // Nếu callback thất bại, cập nhật trạng thái Order và Payment, đồng thời hoàn trả số lượng sản phẩm đã khóa
                {
                    order.Payment.PaymentStatus = PaymentStatus.FAILED;
                    order.status = OrderStatus.CANCELLED;
                    order.CancelReason = $"Thanh toán thất bại (VNPay: {callback.ResponseCode}/{callback.TransactionStatus})";
                    order.CancelledAt = DateTime.UtcNow;

                    foreach (var group in order.InventoryLocks
                                 .Where(x => x.Status == InventoryLockStatus.LOCKED)
                                 .GroupBy(x => x.ProductColorId)
                                 .OrderBy(x => x.Key))
                    {
                        var productColor = await _unitOfWork.ProductColorRepository.GetForUpdateAsync(group.Key);
                        if (productColor != null)
                        {
                            productColor.Quantity += group.Sum(x => x.Quantity);
                            _unitOfWork.ProductColorRepository.Update(productColor);
                        }
                        foreach (var inventoryLock in group)
                            inventoryLock.Status = InventoryLockStatus.REBASE;
                    }

                    foreach (var usage in order.DiscountUsages.ToList())
                        _unitOfWork.DiscountUsageRepository.Delete(usage);

                    AddOutboxMessage("payment.failed", order, callback);
                }

                _unitOfWork.OrderRepository.Update(order);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
                return new("00", "Confirm Success");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private void AddOutboxMessage(string eventType, Order order, VNPayCallbackResultDTO callback)
        {
            _unitOfWork.OutboxMessageRepository.Add(new OutboxMessage
            {
                EventType = eventType,
                Payload = JsonSerializer.Serialize(new
                {
                    EventId = Guid.NewGuid(),
                    EventType = eventType,
                    Data = new
                    {
                        OrderId = order.Id,
                        PaymentId = order.Payment.Id,
                        order.MemberId,
                        Amount = order.Payment.TotalPrice,
                        PaymentMethod = order.Payment.PaymentMethod.ToString(),
                        order.Payment.TransactionId,
                        callback.ResponseCode,
                        callback.TransactionStatus,
                        OccurredAt = DateTime.UtcNow
                    }
                })
            });
        }
    }
}
