using AutoMapper;
using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.Exceptions;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Services
{
    public class PetVariantService : IPetVariantService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PetVariantService(IUnitOfWork unitOfWork, IMapper mapper )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task CreatePetVariantAsync(int productId, CreatePetDTO createPetDTO)
        {
            var petVariant = new PetVariant
            {
                ProductId = productId,
                Gender = createPetDTO.Gender,
                Size = createPetDTO.Size,
                Weight = createPetDTO.Weight
            };
            await _unitOfWork.PetVariantRepository.Add(petVariant);
        }

        public async Task UpdatePetVariantAsync(UpdatePetDTO updatePetDTO, int productId)
        {
            var petVariant = await _unitOfWork.PetVariantRepository.GetPetvariantByProductIdAsync(productId);
            if (petVariant == null)
            {
                throw new NotFoundException($"Pet variant with ID {productId} not found.");
            }
            MapUpdatePetVariant(updatePetDTO, petVariant);

            await _unitOfWork.PetVariantRepository.Update(petVariant);
        }
        private void MapUpdatePetVariant(UpdatePetDTO updatePetDTO, PetVariant petVariant)
        {
            if (updatePetDTO.Gender != null)
            {
                petVariant.Gender = updatePetDTO.Gender;
            }
            if (updatePetDTO.Size != null)
            {
                petVariant.Size = updatePetDTO.Size;
            }
            if (updatePetDTO.Weight != 0)
            {
                petVariant.Weight = updatePetDTO.Weight;
            }
        }
        public async Task<PetResponseRequestDTO> GetPetVariantByIdAsync(int productId)
        {
            var petVariant = await _unitOfWork.PetVariantRepository.GetPetVariantAsync(productId);
            if(petVariant == null)
            {
                throw new NotFoundException($"Pet with ID {productId} not found.");
            }
            var petResponseRequestDTO = new PetResponseRequestDTO();
            _mapper.Map(petVariant.Product, petResponseRequestDTO);
            petResponseRequestDTO.Gender = petVariant.Gender;
            petResponseRequestDTO.Size = petVariant.Size;
            petResponseRequestDTO.Weight = petVariant.Weight;
            return petResponseRequestDTO;
        }
        public async Task<IEnumerable<ProductItemsDTO>> GetPetVariantByFillerAsync(
            PetFillerDTO petFilterDTO, int page, int pageSize)
        {
            var petVariants = await _unitOfWork.PetVariantRepository
                .GetPetVariantByFillerAsync(petFilterDTO, page, pageSize);
            if(!petVariants.Any())
            {
                Enumerable.Empty<ProductItemsDTO>();
            }
            var products = petVariants.Select(p => p.Product).ToList();
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

        public async Task<IEnumerable<AdminPetVariantItemDTO>> AdminGetPetVariantByFillerAsync(
            AdminPetFillerDTO petFilterDTO, int page, int pageSize)
        {
            var petVariants = await _unitOfWork.PetVariantRepository
                .AdminGetPetVariantByFillerAsync(petFilterDTO, page, pageSize);
            if(!petVariants.Any())
                return Enumerable.Empty<AdminPetVariantItemDTO>();
            var petVariantItemDTOs = petVariants.Select(petVariant =>
            {
                var petVariantItemDTO = new AdminPetVariantItemDTO();
                _mapper.Map(petVariant.Product, petVariantItemDTO);
                return petVariantItemDTO;
            }).ToList();

            return petVariantItemDTOs;
        }
    }
}
