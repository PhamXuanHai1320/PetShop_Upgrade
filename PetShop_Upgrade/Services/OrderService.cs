using Microsoft.EntityFrameworkCore;
using Minio.DataModel;
using PetShop_Upgrade.DTOS.Order;
using PetShop_Upgrade.Exceptions;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using PetShop_Upgrade.Services.Interfaces;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAddressDataService _addressDataService;
        private readonly ILogger<OrderService> _logger;
        private static readonly Dictionary<OrderStatus, OrderStatus[]> ValidTransitions = new()
        {
            [OrderStatus.PENDING] = new[] { OrderStatus.CONFIRMED },
            [OrderStatus.CONFIRMED] = new[] { OrderStatus.SHIPPED },
            [OrderStatus.SHIPPED] = new[] { OrderStatus.DELIVERED },
            [OrderStatus.DELIVERED] = Array.Empty<OrderStatus>(),
            [OrderStatus.CANCELLED] = Array.Empty<OrderStatus>(),
        };

        public OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger, IAddressDataService addressDataService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _addressDataService = addressDataService;
        }
        public async Task<CreateOrderResultDTO> CreateOrderFromCartAsync(int memberId, CreateOrderFromCartRequestDTO createOrderRequestDTO)
        {
            if (createOrderRequestDTO.CartItemIds == null || createOrderRequestDTO.CartItemIds.Count == 0)
                throw new BadRequestException("Vui lòng chọn ít nhất 1 sản phẩm trong giỏ hàng");
            var cartItems = await _unitOfWork.CartItemRepository
                .FindAsync(ci => createOrderRequestDTO.CartItemIds.Contains(ci.Id) && ci.Cart.MemberId == memberId);

            if (cartItems.Count() != createOrderRequestDTO.CartItemIds.Distinct().Count())
                throw new BadRequestException("Một số sản phẩm trong giỏ hàng không hợp lệ");

            // Biến kiểm tra xem đơn hàng có phải là COD hay không, để quyết định có cần lock row hay không
            var isCod = createOrderRequestDTO.PaymentMethod == PaymentMethod.CASH;

            /*  Sắp xếp theo ProductColorId tăng dần để mọi transaction
                luôn lock các row theo cùng 1 thứ tự tránh deadlock chéo nhau */
            var sortedItems = cartItems.OrderBy(x => x.ProductColorId).ToList();

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Lấy danh sách ProductId từ các item, và truy vấn Product
                var productIds = sortedItems.Select(i => i.ProductId).Distinct().ToList();
                var products = await _unitOfWork.ProductRepository.GetProductsByIdsAsync(productIds);
                if (products.Count() != productIds.Distinct().Count())
                    throw new NotFoundException("Một số sản phẩm không tồn tại");

                // Tạo dictionary để tra cứu nhanh
                var productDict = products.ToDictionary(p => p.Id);
                var discount = await _unitOfWork.DiscountRepository.GetDiscountByIdAsync(createOrderRequestDTO.DiscountId);

                // Lấy danh sách ProductColorId từ các item, và truy vấn ProductColor với lock
                var productColorIds = sortedItems.Select(i => i.ProductColorId).Distinct().ToList();
                var productColors = await _unitOfWork.ProductColorRepository.GetForUpdateBatchAsync(productColorIds);
                var productColorDict = productColors.ToDictionary(pc => pc.Id);

                // Kiểm tra địa chỉ giao hàng có tồn tại và thuộc về memberId không
                var address = await _unitOfWork.AddressRepository.GetById(createOrderRequestDTO.AddressId);
                if (address is null || address.MemberId != memberId)
                    throw new BadRequestException("Địa chỉ giao hàng không hợp lệ.");
                // Lấy thông tin tên thành phố và tên phường/xã từ AddressDataService
                var city = _addressDataService.GetCityDetail(address.City);
                var ward = _addressDataService.GetWardsByCityCode(address.City)
                                ?.FirstOrDefault(w => w.Code == address.Ward);

                var order = new Order
                {
                    MemberId = memberId,
                    AddressId = createOrderRequestDTO.AddressId,
                    Note = createOrderRequestDTO.Note ?? "",
                    status = isCod ? OrderStatus.CONFIRMED : OrderStatus.PENDING,
                    CreatedAt = DateTime.Now,
                    ShippingAddressDetail = address.AddressDetail,
                    ShippingCityName = city?.Name ?? String.Empty,
                    ShippingCityCode = address.City ?? String.Empty,
                    ShippingWardName = ward?.Name ?? String.Empty,
                    ShippingWardCode = address.Ward ?? String.Empty,
                    ShippingPhoneNumber = address.PhoneNumber
                };

                decimal totalPrice = 0;
                decimal applicablePrice = 0;

                foreach (var item in sortedItems)
                {
                    if (!productDict.TryGetValue(item.ProductId, out var product))
                        throw new NotFoundException($"Không tìm thấy sản phẩm (ProductId = {item.ProductId})");

                    if (!productColorDict.TryGetValue(item.ProductColorId, out var productColor))
                        throw new NotFoundException($"Không tìm thấy biến thể sản phẩm (ProductColorId = {item.ProductColorId})");

                    // Check ProductColorId có thuộc ProductId không
                    if (productColor.ProductId != item.ProductId)
                        throw new BadRequestException($"ProductColorId = {item.ProductColorId} không thuộc ProductId = {item.ProductId}");

                    // Kiểm tra tồn kho sau khi lock
                    if (productColor.Quantity < item.Quantity)
                        throw new BadRequestException(
                            $"Sản phẩm '{product.Id}' chỉ còn {productColor.Quantity} sản phẩm, không đủ số lượng yêu cầu ({item.Quantity})");

                    // Trừ kho ngay trong lúc đang giữ lock
                    productColor.Quantity -= item.Quantity;
                    _unitOfWork.ProductColorRepository.Update(productColor);

                    // Tạo OrderDetail
                    var sellingPrice = product.SellingPrice;
                    var importPrice = product.ImportPrice;
                    var itemTotal = sellingPrice * item.Quantity;

                    var orderDetail = new OrderDetail
                    {
                        ProductId = product.Id,
                        ProductColorId = item.ProductColorId,
                        Quantity = item.Quantity,
                        SellingPrice = product.SellingPrice,
                        ImportPrice = importPrice,
                        TotalPrice = itemTotal,
                        Order = order
                    };

                    totalPrice += itemTotal;
                    order.OrderDetails.Add(orderDetail);

                    // Tính tổng tiền các món áp được mã giảm giá (nếu có)
                    if (discount != null && IsDiscountApplicable(discount, product))
                        applicablePrice += itemTotal;

                    // Ghi InventoryLock để giữ chỗ - chờ thanh toán trong 10 phút, chỉ khi không phải COD
                    if (!isCod)
                    {
                        var inventoryLock = new InventoryLock
                        {
                            Quantity = item.Quantity,
                            Status = InventoryLockStatus.LOCKED,
                            CreatedAt = DateTime.UtcNow,
                            ExpireAt = DateTime.UtcNow.AddMinutes(10),
                            ProductColorId = item.ProductColorId,
                            Order = order
                        };
                        order.InventoryLocks.Add(inventoryLock);
                    }
                }

                var discountPrice = CalculateDiscount(discount, applicablePrice);

                order.TotalPrice = totalPrice;
                order.DiscountPrice = discountPrice;
                order.FinalPrice = totalPrice - discountPrice;

                if (discount != null && discountPrice > 0)
                {
                    order.DiscountUsages.Add(new DiscountUsage
                    {
                        DiscountId = discount.Id,
                        MemberId = memberId,
                        Order = order
                    });
                }

                var user = await _unitOfWork.MemberRepository.GetById(memberId);

                // Khởi tạo bản ghi Payment cho đơn COD
                order.Payment= new Payment
                {
                    TotalPrice = order.FinalPrice,
                    PaymentMethod = createOrderRequestDTO.PaymentMethod,
                    PaymentStatus = PaymentStatus.PENDING,
                    TransactionId = null,
                    PaidAt = null,
                    CreateAt = DateTime.Now,
                    BuyerName = user.FirstName + "" + user.LastName,
                    BuyerEmail = user.Email,
                    BuyerPhone = user.PhoneNumber
                };
                _unitOfWork.OrderRepository.Add(order);
                foreach (var ci in cartItems)
                    _unitOfWork.CartItemRepository.Delete(ci);
                await _unitOfWork.SaveChangesAsync();

                // Chỉ khi commit thành công, lock trên các row mới được nhả ra
                await transaction.CommitAsync();

                _logger.LogInformation("Tạo đơn hàng thành công, OrderId = {OrderId}", order.Id);
                return new CreateOrderResultDTO
                {
                    OrderId = order.Id,
                    FinalPrice = order.FinalPrice
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, "Rollback transaction khi tạo đơn hàng cho MemberId = {MemberId}", memberId);
                throw; // Ném lại để GlobalExceptionMiddleware xử lý response cho client
            }
        }
        public async Task<CreateOrderResultDTO> CreateOrderAsync(int memberId, CreateOrderItemRequestDTO createOrderRequestDTO)
        {
            if (createOrderRequestDTO.Quantity <= 0)
                throw new BadRequestException("Số lượng sản phẩm phải lớn hơn 0");

            var isCod = createOrderRequestDTO.PaymentMethod == PaymentMethod.CASH;

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(createOrderRequestDTO.ProductId);
                if (product == null)
                    throw new NotFoundException($"Không tìm thấy sản phẩm (ProductId = {createOrderRequestDTO.ProductId})");

                if (product.Type == ProductType.Pet)
                {
                    var completedAppointments = await _unitOfWork.PetViewingAppointmentRepository.FindAsync(
                        pva => pva.ProductId == product.Id &&
                               pva.Appointment.MemberId == memberId &&
                               pva.Appointment.Status == AppointmentStatus.COMPLETED);

                    if (!completedAppointments.Any())
                        throw new BadRequestException(
                            $"Bạn cần hoàn thành lịch hẹn xem thú cưng {product.ProductName} trước khi mua");
                }

                var productColor = await _unitOfWork.ProductColorRepository.GetForUpdateAsync(createOrderRequestDTO.ProductColorId);
                if (productColor == null)
                    throw new NotFoundException($"Không tìm thấy biến thể sản phẩm (ProductColorId = {createOrderRequestDTO.ProductColorId})");
                if (productColor.ProductId != createOrderRequestDTO.ProductId)
                    throw new BadRequestException($"ProductColorId = {createOrderRequestDTO.ProductColorId} không thuộc ProductId = {createOrderRequestDTO.ProductId}");
                // Kiểm tra tồn kho sau khi lock
                if (productColor.Quantity < createOrderRequestDTO.Quantity)
                    throw new BadRequestException($"Sản phẩm '{product.Id}' chỉ còn {productColor.Quantity} sản phẩm, không đủ số lượng yêu cầu ({createOrderRequestDTO.Quantity})");

                // Trừ kho ngay trong lúc đang giữ lock
                productColor.Quantity -= createOrderRequestDTO.Quantity;
                _unitOfWork.ProductColorRepository.Update(productColor);

                // Kiểm tra địa chỉ giao hàng có tồn tại và thuộc về memberId không
                var address = await _unitOfWork.AddressRepository.GetById(createOrderRequestDTO.AddressId);
                if (address is null || address.MemberId != memberId)
                    throw new BadRequestException("Địa chỉ giao hàng không hợp lệ.");
                // Lấy thông tin tên thành phố và tên phường/xã từ AddressDataService
                var city = _addressDataService.GetCityDetail(address.City);
                var ward = _addressDataService.GetWardsByCityCode(address.City)
                                ?.FirstOrDefault(w => w.Code == address.Ward);

                var discount = await _unitOfWork.DiscountRepository.GetDiscountByIdAsync(createOrderRequestDTO.DiscountId);

                decimal totalPrice = product.SellingPrice * createOrderRequestDTO.Quantity;
                decimal discountPrice = 0;

                if (discount != null && IsDiscountApplicable(discount, product))
                    discountPrice = CalculateDiscount(discount, totalPrice);

                var orderDetail = new OrderDetail
                {
                    ProductId = product.Id,
                    ProductColorId = productColor.Id,
                    Quantity = createOrderRequestDTO.Quantity,
                    SellingPrice = product.SellingPrice,
                    ImportPrice = product.ImportPrice,
                    TotalPrice = totalPrice
                };
                var user = await _unitOfWork.MemberRepository.GetById(memberId);

                // Khởi tạo bản ghi Payment cho đơn COD
                var payment = new Payment
                {
                    TotalPrice = totalPrice - discountPrice,
                    PaymentMethod = createOrderRequestDTO.PaymentMethod,
                    PaymentStatus = PaymentStatus.PENDING,
                    TransactionId = null,
                    PaidAt = null,
                    CreateAt = DateTime.Now,
                    BuyerName = user.FirstName + "" + user.LastName,
                    BuyerEmail = user.Email,
                    BuyerPhone = user.PhoneNumber
                };

                var order = new Order
                {
                    MemberId = memberId,
                    AddressId = createOrderRequestDTO.AddressId,
                    TotalPrice = totalPrice,
                    DiscountPrice = discountPrice,
                    FinalPrice = totalPrice - discountPrice,
                    Note = createOrderRequestDTO.Note ?? "",
                    status = isCod ? OrderStatus.CONFIRMED : OrderStatus.PENDING,
                    CreatedAt = DateTime.Now,
                    ShippingAddressDetail = address.AddressDetail,
                    ShippingCityName = city?.Name ?? String.Empty,
                    ShippingCityCode = address.City ?? String.Empty,
                    ShippingWardName = ward?.Name ?? String.Empty,
                    ShippingWardCode = address.Ward ?? String.Empty,
                    ShippingPhoneNumber = address.PhoneNumber,
                    OrderDetails = new List<OrderDetail> { orderDetail },
                    Payment = payment
                };

                // Ghi InventoryLock để giữ chỗ - chờ thanh toán trong 10 phút, chỉ khi không phải COD
                if (!isCod)
                {
                    order.InventoryLocks.Add(new InventoryLock
                    {
                        Quantity = createOrderRequestDTO.Quantity,
                        Status = InventoryLockStatus.LOCKED,
                        CreatedAt = DateTime.UtcNow,
                        ExpireAt = DateTime.UtcNow.AddMinutes(10),
                        ProductColorId = productColor.Id
                    });
                }

                if (discount != null && discountPrice > 0)
                {
                    order.DiscountUsages.Add(new DiscountUsage
                    {
                        DiscountId = discount.Id,
                        MemberId = memberId,
                        Order = order
                    });
                }
                _unitOfWork.OrderRepository.Add(order);
                await _unitOfWork.SaveChangesAsync();
                // Chỉ khi commit thành công, lock trên các row mới được nhả ra
                await transaction.CommitAsync();
                _logger.LogInformation("Tạo đơn hàng thành công, OrderId = {OrderId}", order.Id);
                return new CreateOrderResultDTO
                {
                    OrderId = order.Id,
                    FinalPrice = order.FinalPrice
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, "Rollback transaction khi tạo đơn hàng cho MemberId = {MemberId}", memberId);
                throw; // Ném lại để GlobalExceptionMiddleware xử lý response cho client
            }
        }

        public async Task<OrderPreviewResponseDTO> OrderPreview(OrderPreviewRequestDTO orderPreviewRequestDTO)
        {
            var discount = await _unitOfWork.DiscountRepository.GetDiscountByIdAsync(orderPreviewRequestDTO.DiscountId);

            var productIds = orderPreviewRequestDTO.Items.Select(i => i.ProductId).ToList();
            var products = await _unitOfWork.ProductRepository.GetProductsByIdsAsync(productIds);
            if (products.Count() != productIds.Distinct().Count())
                throw new NotFoundException("Một số sản phẩm không tồn tại");
            var productDict = products.ToDictionary(p => p.Id);

            decimal totalPrice = 0;
            decimal applicablePrice = 0; // tổng tiền các món áp được mã

            foreach (var item in orderPreviewRequestDTO.Items)
            {
                if (!productDict.TryGetValue(item.ProductId, out var product))
                    throw new NotFoundException($"Không tìm thấy sản phẩm (ProductId = {item.ProductId})");

                decimal itemPrice = product.SellingPrice * item.Quantity;
                totalPrice += itemPrice;

                if (discount != null && IsDiscountApplicable(discount, product))
                    applicablePrice += itemPrice;
            }

            var discountPrice = CalculateDiscount(discount, applicablePrice);

            return new OrderPreviewResponseDTO
            {
                TotalPrice = totalPrice,
                DiscountPrice = discountPrice,
                FinalPrice = totalPrice - discountPrice
            };
        }
        private static bool IsDiscountApplicable(Discount discount, Product product)
        {
            return discount.Scope == DiscountScope.ORDER ||
                   discount.DiscountCategories.Any(dc => dc.CategoryId == product.CategoryId) ||
                   discount.DiscountProducts.Any(dp => dp.ProductId == product.Id);
        }

        private static decimal CalculateDiscount(Discount? discount, decimal applicablePrice)
        {
            if (discount == null || applicablePrice <= 0)
                return 0;

            if (discount.MinOrderValue.HasValue && applicablePrice < discount.MinOrderValue.Value)
                return 0;

            decimal discountPrice = discount.DiscountType == DiscountType.PERCENTAGE
                ? applicablePrice * (discount.DiscountValue / 100)
                : discount.DiscountValue;

            if (discount.DiscountType == DiscountType.PERCENTAGE && discount.MaxDiscountAmount.HasValue)
                discountPrice = Math.Min(discountPrice, discount.MaxDiscountAmount.Value);

            return Math.Min(discountPrice, applicablePrice);
        }
        // ADMIN: hủy đơn của khách
        public async Task CancelOrderByAdminAsync(int orderId, int adminId, CancelOrderRequestDTO dto)
        {
            await CancelOrderInternalAsync(orderId, dto.CancelReason, cancelledByAdminId: adminId, requestingMemberId: null);
            _logger.LogInformation("OrderId = {OrderId} đã bị hủy bởi AdminId = {AdminId}", orderId, adminId);
        }

        // MEMBER: tự hủy đơn của chính mình
        public async Task CancelOrderByMemberAsync(int orderId, int memberId, CancelOrderRequestDTO dto)
        {
            await CancelOrderInternalAsync(orderId, dto.CancelReason, cancelledByAdminId: null, requestingMemberId: memberId);
            _logger.LogInformation("OrderId = {OrderId} đã được MemberId = {MemberId} tự hủy", orderId, memberId);
        }

        public async Task CancelOrderBySystemAsync(int orderId, string CancelReason)
        {
            await CancelOrderInternalAsync(orderId, CancelReason, cancelledByAdminId: null, requestingMemberId: null);
            _logger.LogInformation(CancelReason);
        }
        private async Task CancelOrderInternalAsync(
            int orderId, string cancelReason, int? cancelledByAdminId, int? requestingMemberId)
        {
            if (string.IsNullOrWhiteSpace(cancelReason))
                throw new BadRequestException("Vui lòng nhập lý do hủy đơn");

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = await _unitOfWork.OrderRepository.GetOrderForUpdateAsync(orderId);
                if (order == null)
                    throw new NotFoundException($"Không tìm thấy đơn hàng (OrderId = {orderId})");

                if (order.status == OrderStatus.CANCELLED)
                {
                    if (!requestingMemberId.HasValue && !cancelledByAdminId.HasValue)
                    {
                        await transaction.CommitAsync();
                        return;
                    }
                    throw new BadRequestException("Đơn hàng đã được hủy trước đó");
                }

                if (requestingMemberId.HasValue && order.MemberId != requestingMemberId.Value)
                    throw new ForbiddenException("Bạn không có quyền hủy đơn hàng này");

                var isCod = order.Payment?.PaymentMethod == PaymentMethod.CASH;

                if (requestingMemberId.HasValue)
                {
                    // COD: được hủy khi còn CONFIRMED (chưa giao)
                    // Online: chỉ được hủy khi còn PENDING (chưa thanh toán)
                    bool canMemberCancel = isCod
                        ? order.status == OrderStatus.CONFIRMED
                        : order.status == OrderStatus.PENDING;

                    if (!canMemberCancel)
                        throw new BadRequestException("Đơn hàng hiện không thể hủy");
                }
                else if(cancelledByAdminId.HasValue)
                {
                    // Admin: không cho hủy khi đã DELIVERED hoặc CANCELLED
                    if (order.status is OrderStatus.DELIVERED or OrderStatus.CANCELLED or OrderStatus.SHIPPED)
                        throw new BadRequestException($"Không thể hủy đơn hàng đang ở trạng thái {order.status}");
                }

                // InventoryLock hoàn kho
                var sortedDetails = order.OrderDetails.OrderBy(d => d.ProductColorId).ToList();
                foreach (var detail in sortedDetails)
                {
                    var productColor = await _unitOfWork.ProductColorRepository.GetForUpdateAsync(detail.ProductColorId);
                    if (productColor == null)
                        throw new NotFoundException($"Không tìm thấy biến thể sản phẩm (ProductColorId = {detail.ProductColorId})");

                    productColor.Quantity += detail.Quantity;
                    _unitOfWork.ProductColorRepository.Update(productColor);
                }

                foreach (var invLock in order.InventoryLocks.Where(l => l.Status != InventoryLockStatus.REBASE))
                {
                    invLock.Status = InventoryLockStatus.REBASE;
                }

                // Xóa DiscountUsage nếu có, vì đơn bị hủy nên không còn áp dụng mã giảm giá nữa
                if (order.DiscountUsages.Any())
                {
                    foreach (var discountUsage in order.DiscountUsages)
                    {
                        _unitOfWork.DiscountUsageRepository.Delete(discountUsage);
                    }
                }
                // Cập nhật trạng thái đơn hàng
                order.status = OrderStatus.CANCELLED;
                order.CancelReason = cancelReason;
                order.CancelledAt = DateTime.Now;
                order.CancelledByAdminId = cancelledByAdminId;

                _unitOfWork.OrderRepository.Update(order);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, "Rollback transaction khi hủy đơn hàng OrderId = {OrderId}", orderId);
                throw;
            }
        }
        public async Task UpdateOrderStatusAsync(int orderId, int adminId, UpdateOrderStatusRequestDTO updateOrderDTO)
        {
            if (updateOrderDTO.NewStatus == OrderStatus.CANCELLED)
                throw new BadRequestException("Vui lòng dùng chức năng hủy đơn để chuyển sang trạng thái này");

            var order = await _unitOfWork.OrderRepository.GetOrderWithDetailsByIdAsync(orderId);
            if (order == null)
                throw new NotFoundException($"Không tìm thấy đơn hàng (OrderId = {orderId})");

            var allowedNextStatuses = ValidTransitions.GetValueOrDefault(order.status, Array.Empty<OrderStatus>());
            if (!allowedNextStatuses.Contains(updateOrderDTO.NewStatus))
                throw new BadRequestException($"Không thể chuyển trạng thái từ {order.status} sang {updateOrderDTO.NewStatus}");

            if (updateOrderDTO.NewStatus == OrderStatus.CONFIRMED &&
                order.Payment?.PaymentMethod == PaymentMethod.VNPAY &&
                order.Payment.PaymentStatus != PaymentStatus.PAID)
                throw new BadRequestException("Không thể xác nhận đơn VNPay khi thanh toán chưa thành công");

            order.status = updateOrderDTO.NewStatus;
            if (updateOrderDTO.NewStatus == OrderStatus.DELIVERED &&
                order.Payment?.PaymentMethod == PaymentMethod.CASH &&
                order.Payment.PaymentStatus == PaymentStatus.PENDING)
            {
                order.Payment.PaymentStatus = PaymentStatus.PAID;
                order.Payment.PaidAt = DateTime.UtcNow;
            }
            _unitOfWork.OrderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "AdminId = {AdminId} đã đổi trạng thái OrderId = {OrderId} sang {NewStatus}",
                adminId, orderId, updateOrderDTO.NewStatus);
        }
        public async Task ConfirmOrderPaymentAsync(int orderId)
        {
            var order = await _unitOfWork.OrderRepository.GetOrderWithDetailsByIdAsync(orderId);
            if (order == null || order.status != OrderStatus.PENDING)
                return;

            order.status = OrderStatus.CONFIRMED;
            foreach (var invLock in order.InventoryLocks.Where(l => l.Status == InventoryLockStatus.LOCKED))
            {
                invLock.Status = InventoryLockStatus.CONFIRMED;
            }

            _unitOfWork.OrderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ExpirePendingVNPayOrdersAsync(CancellationToken cancellationToken = default)
        {
            var orderIds = await _unitOfWork.InventoryLockRepository
                .GetExpiredPendingOrderIdsAsync(DateTime.UtcNow);

            foreach (var orderId in orderIds)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var order = await _unitOfWork.OrderRepository.GetOrderForUpdateAsync(orderId);
                    if (order?.Payment == null ||
                        order.status != OrderStatus.PENDING ||
                        order.Payment.PaymentMethod != PaymentMethod.VNPAY ||
                        order.Payment.PaymentStatus != PaymentStatus.PENDING)
                    {
                        await transaction.CommitAsync();
                        continue;
                    }

                    var expiredLocks = order.InventoryLocks
                        .Where(x => x.Status == InventoryLockStatus.LOCKED)
                        .ToList();
                    if (expiredLocks.Count == 0)
                    {
                        await transaction.CommitAsync();
                        continue;
                    }

                    foreach (var group in expiredLocks.GroupBy(x => x.ProductColorId).OrderBy(x => x.Key))
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

                    order.Payment.PaymentStatus = PaymentStatus.FAILED;
                    order.Payment.CancelledAt = DateTime.UtcNow;
                    order.Payment.CancellationReason = "Hết thời gian thanh toán VNPay";
                    order.status = OrderStatus.CANCELLED;
                    order.CancelReason = "Hết thời gian thanh toán VNPay";
                    order.CancelledAt = DateTime.UtcNow;
                    foreach (var usage in order.DiscountUsages.ToList())
                        _unitOfWork.DiscountUsageRepository.Delete(usage);

                    _unitOfWork.OrderRepository.Update(order);
                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}
