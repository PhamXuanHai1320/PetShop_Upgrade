using AutoMapper;
using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DiscountService(IDiscountRepository discountRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _discountRepository = discountRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DiscountDTO>> GetAllDiscountsAsync()
        {
            var discounts = await _discountRepository.GetAllDiscountsAsync();
            return _mapper.Map<IEnumerable<DiscountDTO>>(discounts);
        }

        public async Task<DiscountDTO> GetDiscountByIdAsync(int id)
        {
            var discount = await _discountRepository.GetDiscountByIdAsync(id);
            if (discount == null) throw new KeyNotFoundException($"Không tìm thấy discount với Id = {id}");
            return _mapper.Map<DiscountDTO>(discount);
        }

        public async Task<IEnumerable<DiscountDTO>> GetDiscountByNameAsync(string discountName)
        {
            var discounts = await _discountRepository.GetDiscountsByNameAsync(discountName);
            if (discounts == null || !discounts.Any()) 
                throw new KeyNotFoundException($"Không tìm thấy discount với tên = {discountName}");
            return _mapper.Map<IEnumerable<DiscountDTO>>(discounts);
        }

        public async Task<DiscountDTO> GetDiscountByCodeAsync(string code)
        {
            var discount = await _discountRepository.GetDiscountsByCodeAsync(code);
            if (discount == null) throw new KeyNotFoundException($"Không tìm thấy discount với mã = {code}");
            return _mapper.Map<DiscountDTO>(discount);
        }

        public async Task<IEnumerable<DiscountDTO>> GetDiscountByFillerAsync(DiscountFilterDTO discountFilterDTO)
        {
            var discounts = await _discountRepository.GetDiscountsByFillerAsync(discountFilterDTO);
            return _mapper.Map<IEnumerable<DiscountDTO>>(discounts);
        }

        public async Task<IEnumerable<DiscountDTO>> GetDiscountsByProductIdAsync(int productId)
        {
            var discounts = await _discountRepository.GetDiscountsByProductIdAsync(productId);
            if(discounts == null || !discounts.Any())
                throw new KeyNotFoundException($"Không tìm thấy discount nào áp dụng cho productId = {productId}"); 
            return _mapper.Map<IEnumerable<DiscountDTO>>(discounts);
        }

        public async Task<IEnumerable<DiscountDTO>> GetDiscountsByCategoryIdAsync(int categoryId)
        {
            var discounts = await _discountRepository.GetDiscountsByCategoryIdAsync(categoryId);
            if (discounts == null || !discounts.Any())
                throw new KeyNotFoundException($"Không tìm thấy discount nào áp dụng cho categoryId = {categoryId}");
            return _mapper.Map<IEnumerable<DiscountDTO>>(discounts);
        }

        public async Task<CreateDiscountDTO> CreateDiscountAsync(CreateDiscountDTO createDiscountDTO)
        {
            // Check duplicate Code
            var existed = await _discountRepository.GetDiscountsByCodeAsync(createDiscountDTO.Code);
            if (existed != null) throw new InvalidOperationException($"Mã discount '{createDiscountDTO.Code}' đã tồn tại");

            var discount = _mapper.Map<Discount>(createDiscountDTO);

            // Gắn DiscountProducts
            discount.DiscountProducts = createDiscountDTO.ProductIds
                .Select(id => new DiscountProduct { ProductId = id })
                .ToList();

            // Gắn DiscountCategories
            discount.DiscountCategories = createDiscountDTO.CategoryIds
                .Select(id => new DiscountCategory { CategoryId = id })
                .ToList();

            await _discountRepository.Add(discount);
            await _unitOfWork.SaveChangesAsync();

            return createDiscountDTO;
        }

        public async Task<CreateDiscountDTO> UpdateDiscountAsync(int id, CreateDiscountDTO createDiscountDTO)
        {
            var discount = await _discountRepository.GetDiscountByIdAsync(id);
            if (discount == null) throw new KeyNotFoundException($"Không tìm thấy discount với Id = {id}");

            if (discount.Code != createDiscountDTO.Code)
            {
                var existed = await _discountRepository.GetDiscountsByCodeAsync(createDiscountDTO.Code);
                if (existed != null) throw new InvalidOperationException($"Mã discount '{createDiscountDTO.Code}' đã tồn tại");
            }

            _mapper.Map(createDiscountDTO, discount);

            discount.DiscountProducts = createDiscountDTO.ProductIds
                .Select(pid => new DiscountProduct { ProductId = pid, DiscountId = id })
                .ToList();

            discount.DiscountCategories = createDiscountDTO.CategoryIds
                .Select(cid => new DiscountCategory { CategoryId = cid, DiscountId = id })
                .ToList();

            _discountRepository.Update(discount);
            await _unitOfWork.SaveChangesAsync();

            return createDiscountDTO;
        }

        public async Task DeleteDiscountAsync(int id)
        {
            var discount = await _discountRepository.GetDiscountByIdAsync(id);
            if (discount == null) throw new KeyNotFoundException($"Không tìm thấy discount với Id = {id}");

            // Check còn liên kết Product hoặc Category không
            if (discount.DiscountProducts.Any() || discount.DiscountCategories.Any())
                throw new InvalidOperationException("Không thể vô hiệu hóa discount đang được liên kết với sản phẩm hoặc danh mục");

            discount.IsActive = 0;
            _discountRepository.Update(discount);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
