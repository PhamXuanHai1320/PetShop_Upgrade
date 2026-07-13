using AutoMapper;
using PetShop_Upgrade.DTOS.Discounts;
using PetShop_Upgrade.DTOS.Order.Client;
using PetShop_Upgrade.Exceptions;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using PetShop_Upgrade.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DiscountService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<DiscountDTO> GetDiscountByIdAsync(int id)
        {
            var discount = await _unitOfWork.DiscountRepository.GetDiscountByIdAsync(id);
            if (discount == null) throw new KeyNotFoundException($"Không tìm thấy discount với Id = {id}");
            return _mapper.Map<DiscountDTO>(discount);
        }

        public async Task<DiscountDTO> GetDiscountByCodeAsync(string code)
        {
            var discount = await _unitOfWork.DiscountRepository.GetDiscountsByCodeAsync(code);
            if (discount == null) throw new KeyNotFoundException($"Không tìm thấy discount với mã = {code}");
            return _mapper.Map<DiscountDTO>(discount);
        }

        public async Task<IEnumerable<DiscountDTO>> GetDiscountByFillerAsync(DiscountFilterDTO discountFilterDTO)
        {
            var discounts = await _unitOfWork.DiscountRepository.GetDiscountsByFillerAsync(discountFilterDTO);
            return _mapper.Map<IEnumerable<DiscountDTO>>(discounts);
        }

        public async Task<CreateDiscountDTO> CreateDiscountAsync(CreateDiscountDTO createDiscountDTO)
        {
            //Validate scope
            ValidateDiscountScope(createDiscountDTO);
            // Check duplicate Code
            if (!string.IsNullOrWhiteSpace(createDiscountDTO.Code))
            {
                var existed = await _unitOfWork.DiscountRepository.GetDiscountsByCodeAsync(createDiscountDTO.Code);
                if (existed != null) throw new InvalidOperationException($"Mã discount '{createDiscountDTO.Code}' đã tồn tại");
            }

            var discount = _mapper.Map<Discount>(createDiscountDTO);

            // Gắn DiscountProducts
            discount.DiscountProducts = createDiscountDTO.ProductIds
                .Select(id => new DiscountProduct { ProductId = id })
                .ToList();

            // Gắn DiscountCategories
            discount.DiscountCategories = createDiscountDTO.CategoryIds
                .Select(id => new DiscountCategory { CategoryId = id })
                .ToList();

            _unitOfWork.DiscountRepository.Add(discount);
            await _unitOfWork.SaveChangesAsync();

            return createDiscountDTO;
        }

        public async Task<CreateDiscountDTO> UpdateDiscountAsync(int id, CreateDiscountDTO createDiscountDTO)
        {
            var discount = await _unitOfWork.DiscountRepository.GetDiscountByIdAsync(id);
            if (discount == null) throw new KeyNotFoundException($"Không tìm thấy discount với Id = {id}");

            if (discount.Scope != createDiscountDTO.Scope && discount.DiscountUsages.Any())
                throw new InvalidOperationException(
                    "Không thể đổi Scope của discount đã có lịch sử sử dụng");

            //Validate scope
            ValidateDiscountScope(createDiscountDTO);
            if (discount.Code != createDiscountDTO.Code && !string.IsNullOrWhiteSpace(createDiscountDTO.Code))
            {
                var existed = await _unitOfWork.DiscountRepository.GetDiscountsByCodeAsync(createDiscountDTO.Code);
                if (existed != null) throw new InvalidOperationException($"Mã discount '{createDiscountDTO.Code}' đã tồn tại");
            }

            _mapper.Map(createDiscountDTO, discount);

            discount.DiscountProducts = createDiscountDTO.ProductIds
                .Select(pid => new DiscountProduct { ProductId = pid, DiscountId = id })
                .ToList();

            discount.DiscountCategories = createDiscountDTO.CategoryIds
                .Select(cid => new DiscountCategory { CategoryId = cid, DiscountId = id })
                .ToList();

            _unitOfWork.DiscountRepository.Update(discount);
            await _unitOfWork.SaveChangesAsync();

            return createDiscountDTO;
        }

        public async Task DeleteDiscountAsync(int id)
        {
            var discount = await _unitOfWork.DiscountRepository.GetDiscountByIdAsync(id);
            if (discount == null) throw new KeyNotFoundException($"Không tìm thấy discount với Id = {id}");

            discount.IsActive = 0;
            _unitOfWork.DiscountRepository.Update(discount);
            await _unitOfWork.SaveChangesAsync();
        }
        private void ValidateDiscountScope(CreateDiscountDTO createDiscountDTO)
        {
            switch (createDiscountDTO.Scope)
            {
                case DiscountScope.ORDER:
                    if (string.IsNullOrWhiteSpace(createDiscountDTO.Code))
                        throw new InvalidOperationException(
                            "Discount áp dụng cho toàn đơn hàng (Scope = ORDER) bắt buộc phải có Code");

                    if (createDiscountDTO.ProductIds.Any() || createDiscountDTO.CategoryIds.Any())
                        throw new InvalidOperationException(
                            "Discount áp dụng cho toàn đơn hàng (Scope = ORDER) không được gắn Product hoặc Category");
                    break;

                case DiscountScope.PRODUCT_CATEGORY:
                    if (!createDiscountDTO.ProductIds.Any() && !createDiscountDTO.CategoryIds.Any())
                        throw new InvalidOperationException(
                            "Discount áp dụng cho sản phẩm/danh mục (Scope = PRODUCT_CATEGORY) cần ít nhất 1 Product hoặc Category");
                    break;

                default:
                    throw new InvalidOperationException($"Scope '{createDiscountDTO.Scope}' không hợp lệ");
            }
        }

        public async Task<IEnumerable<DiscountItemsDTO>> GetDiscountsByProductItemsAsync(IEnumerable<CreateOrderItemRequestDTO> OrderItemsDTO)
        {
            var productIds = OrderItemsDTO.Select(i => i.ProductId).ToList();
            
            var products = await _unitOfWork.ProductRepository.GetProductsByIdsAsync(productIds);
            if (products.Count() != productIds.Distinct().Count())
                throw new NotFoundException("Một số sản phẩm không tồn tại");

            decimal totalPrice = products.Sum(p => p.SellingPrice * OrderItemsDTO.First(i => i.ProductId == p.Id).Quantity);
            var currentCategoryIds = products.Select(p => p.CategoryId).Distinct().ToList();

            var discounts = await _unitOfWork.DiscountRepository.GetDiscountsByProductItemsAsync(productIds, currentCategoryIds, totalPrice);

            return _mapper.Map<IEnumerable<DiscountItemsDTO>>(discounts);
        }
    }
}
