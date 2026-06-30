using AutoMapper;
using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.Exceptions;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using PetShop_Upgrade.Services.Interfaces;
using System.Threading.Tasks;

namespace PetShop_Upgrade.Services
{
    public class FoodsDetailService : IFoodsDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FoodsDetailService(IUnitOfWork unitOfWork, IMapper mapper )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task CreateFoodDetailAsync(int productId,CreateFoodDTO createFoodDTO)
        {
            var foodsDetail = new FoodsDetail
            {
                ProductId = productId,
                Flavor = createFoodDTO.Flavor,
                WeightGram = createFoodDTO.WeightGram,
                ExprireDate = createFoodDTO.ExprireDate,
                AgeGroup = createFoodDTO.AgeGroup
            };

            await _unitOfWork.FoodDetailRepository.Add(foodsDetail);
        }
        public async Task UpdateFoodDetailAsync(UpdateFoodDTO updateFoodDTO, int productId)
        {
                var foodDetail = await _unitOfWork.FoodDetailRepository.GetFoodByProductIdAsync(productId);
                if (foodDetail == null)
                {
                    throw new NotFoundException($"Food detail with Product ID {productId} not found.");
                }
                MapUpdateFoodDTO(updateFoodDTO, foodDetail);

                await _unitOfWork.FoodDetailRepository.Update(foodDetail);
        }
        private void MapUpdateFoodDTO(UpdateFoodDTO updateFoodDTO, FoodsDetail foodsDetail)
        {
            if (updateFoodDTO.Flavor != null)
            {
                foodsDetail.Flavor = updateFoodDTO.Flavor;
            }
            if (updateFoodDTO.WeightGram != 0)
            {
                foodsDetail.WeightGram = updateFoodDTO.WeightGram;
            }
            if (updateFoodDTO.ExprireDate.HasValue)
            {
                foodsDetail.ExprireDate = updateFoodDTO.ExprireDate.Value;
            }
            if (updateFoodDTO.AgeGroup != null)
            {
                foodsDetail.AgeGroup = updateFoodDTO.AgeGroup;
            }
        }
        public async Task<FoodResponseRequestDTO> GetFoodDetailByIdAsync(int productId)
        {
            var foodDetail = await _unitOfWork.FoodDetailRepository.GetFoodByProductIdAsync(productId);
            if (foodDetail == null)
            {
                throw new NotFoundException($"Food with ID {productId} not found.");
            }
            var foodResponseRequest = new FoodResponseRequestDTO();
            _mapper.Map(foodDetail.Product, foodResponseRequest);
            foodResponseRequest.Flavor = foodDetail.Flavor;
            foodResponseRequest.WeightGram = foodDetail.WeightGram;
            foodResponseRequest.ExprireDate = foodDetail.ExprireDate;
            foodResponseRequest.AgeGroup = foodDetail.AgeGroup;
            return foodResponseRequest;
        }
        public async Task<IEnumerable<ProductItemsDTO>> GetFoodDetailByFillerAsync(FoodFillerDTO foodFilterDTO, int page, int pageSize)
        {
            var foodDetails = await _unitOfWork.FoodDetailRepository.GetFoodByFillerAsync(foodFilterDTO, page, pageSize);
            var products = foodDetails.Select(p => p.Product).ToList();
            var productIds = products.Select(p => p.Id).ToList();
            var categoryIds = products.Select(p => p.CategoryId).Distinct().ToList();

            var allDiscounts = (await _unitOfWork.DiscountRepository
                .GetDiscountsByProductIdAndCategoryIdAsync(productIds, categoryIds))
                .ToList();

            var productItems = products.Select(product =>
            {
                var applicableDiscounts = allDiscounts.Where(d =>
                    d.DiscountProducts.Any(dp => dp.ProductId == product.Id) ||
                    d.DiscountCategories.Any(dc => dc.CategoryId == product.CategoryId));

                var productItem = _mapper.Map<ProductItemsDTO>(product);
                productItem.FinalPrice = CalculateFinalPriceFromDiscounts(product, applicableDiscounts);
                return productItem;
            }).ToList();

            return productItems;
        }
        private double CalculateFinalPriceFromDiscounts(Product product, IEnumerable<Discount> discounts)
        {
            var eligible = discounts.Where(d =>
                !d.MinOrderValue.HasValue || d.MinOrderValue <= product.SellingPrice);

            if (!eligible.Any())
                return product.SellingPrice;

            double finalPrice = product.SellingPrice;

            foreach (var discount in eligible)
            {
                double discountedPrice;
                if (discount.DiscountType == 0) // Percentage
                {
                    var amount = product.SellingPrice * discount.DiscountValue / 100;
                    if (discount.MaxDiscountAmount.HasValue)
                        amount = Math.Min(amount, discount.MaxDiscountAmount.Value);
                    discountedPrice = product.SellingPrice - amount;
                }
                else // Fixed Amount
                {
                    discountedPrice = product.SellingPrice - discount.DiscountValue;
                }

                if (discountedPrice < finalPrice)
                    finalPrice = discountedPrice;
            }

            return Math.Max(finalPrice, 0);
        }

        public async Task<IEnumerable<AdminFoodItemDTO>> AdminGetFoodDetailByFillerAsync(
            AdminFoodFillerDTO foodFilterDTO, int page, int pageSize)
        {
            var foods = await _unitOfWork.FoodDetailRepository
                .AdminGetFoodDetailByFillerAsync(foodFilterDTO, page, pageSize);
            if (!foods.Any())
                return Enumerable.Empty<AdminFoodItemDTO>();
            var foodItemDTOs = foods.Select(food =>
            {
                var foodItemDTO = new AdminFoodItemDTO();
                _mapper.Map(food.Product, foodItemDTO);
                return foodItemDTO;
            }).ToList();

            return foodItemDTOs;
        }
    }
}
