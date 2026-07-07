using AutoMapper;
using PetShop_Upgrade.DTOS.Products.Client;
using PetShop_Upgrade.DTOS.Toys.Admin;
using PetShop_Upgrade.DTOS.Toys.Client;
using PetShop_Upgrade.Exceptions;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using PetShop_Upgrade.Services.Interfaces;
using System.Threading.Tasks;

namespace PetShop_Upgrade.Services
{
    public class ToysDetailService : IToysDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ToysDetailService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task CreateToyDetailAsync(int productId,CreateToyDTO createToyDTO)
        {
            var toyDetail = new ToysDetail
            {
                ProductId = productId,
                Material = createToyDTO.Material,
                Size = createToyDTO.Size
            };

            _unitOfWork.ToyDetailRepository.Add(toyDetail);
        }
        public async Task UpdateToyDetailAsync(UpdateToyDTO updateToyDTO, int productId)
        {
            var toyDetail = await _unitOfWork.ToyDetailRepository.GetToyByProductIdAsync(productId);
            if (toyDetail == null)
            {
                throw new NotFoundException($"Toys detail for Product ID {productId} not found.");
            }
            MapUpdateToyDetail(updateToyDTO, toyDetail);

            _unitOfWork.ToyDetailRepository.Update(toyDetail);
        }
        private void MapUpdateToyDetail(UpdateToyDTO updateToyDTO, ToysDetail toysDetail)
        {
            if (updateToyDTO.Material != null)
            {
                toysDetail.Material = updateToyDTO.Material;
            }
            if (updateToyDTO.Size != null)
            {
                toysDetail.Size = updateToyDTO.Size;
            }
        }
        public async Task<ToyResponseRequestDTO> GetToyDetailByIdAsync(int productId)
        {
            var toyDetail = await _unitOfWork.ToyDetailRepository.GetToyDetailAsync(productId);
            if (toyDetail == null)
            {
                throw new NotFoundException($"Toy with ID {productId} not found.");
            }
            var toyResponseRequest = new ToyResponseRequestDTO();
            _mapper.Map(toyDetail.Product, toyResponseRequest);
            toyResponseRequest.Material = toyDetail.Material;
            toyResponseRequest.Size = toyDetail.Size;
            return toyResponseRequest;
        }
        public async Task<IEnumerable<ProductItemsDTO>> GetToyDetailByFillerAsync(ToyFilterDTO toyFilterDTO, int page, int pageSize)
        {
            var toyDetails = await _unitOfWork.ToyDetailRepository.GetToyByFillerAsync(toyFilterDTO, page, pageSize);
            var products = toyDetails.Select(p => p.Product).ToList();
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
        private decimal CalculateFinalPriceFromDiscounts(Product product, IEnumerable<Discount> discounts)
        {
            var eligible = discounts.Where(d =>
                !d.MinOrderValue.HasValue || d.MinOrderValue <= product.SellingPrice);

            if (!eligible.Any())
                return product.SellingPrice;

            decimal finalPrice = product.SellingPrice;

            foreach (var discount in eligible)
            {
                decimal discountedPrice;
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

        public async Task<IEnumerable<AdminToyItemDTO>> AdminGetToyDetailByFillerAsync(
            AdminToyFilterDTO toyFilterDTO, int page, int pageSize)
        {
            var toys = await _unitOfWork.ToyDetailRepository
                .AdminGetToyDetailByFillerAsync(toyFilterDTO, page, pageSize);
            if (!toys.Any())
                return Enumerable.Empty<AdminToyItemDTO>();
            var toyItemDTOs = toys.Select(food =>
            {
                var toyItemDTO = new AdminToyItemDTO();
                _mapper.Map(food.Product, toyItemDTO);
                return toyItemDTO;
            }).ToList();

            return toyItemDTOs;
        }
    }
}
