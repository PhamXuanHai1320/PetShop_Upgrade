using AutoMapper;
using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.Exceptions;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Orchestrators.Interfaces;
using PetShop_Upgrade.Repositories.Interfaces;
using PetShop_Upgrade.Services.Interfaces;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Orchestrators
{
    public class ProductOrchestrator : IProductOrchestrator
    {
        private readonly IProductService _productService;
        private readonly IPetVariantService _petService;
        private readonly IToysDetailService _toyService;
        private readonly IFoodsDetailService _foodService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMinioService _minioService;
        private readonly IMapper _mapper;

        public ProductOrchestrator(
            IProductService productService,
            IPetVariantService petService,
            IToysDetailService toyService,
            IFoodsDetailService foodService,
            IUnitOfWork unitOfWork,
            IMinioService minioService,
            IMapper mapper)
        {
            _productService = productService;
            _petService = petService;
            _toyService = toyService;
            _foodService = foodService;
            _unitOfWork = unitOfWork;
            _minioService = minioService;
            _mapper = mapper;
        }

        public async Task CreateFoodAsync(CreateFoodDTO createFoodDTO)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var category = await _unitOfWork.CategoryRepository.GetById(createFoodDTO.CategoryId);
            if(category.ProductType != ProductType.Food) throw new BadRequestException("Category không hợp lệ!");
            try
            {
                int productId = await _productService
                    .CreateBaseProductAsync(createFoodDTO, type: 2);
                await _foodService.CreateFoodDetailAsync(productId, createFoodDTO);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CreatePetAsync(CreatePetDTO createPetDTO)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var category = await _unitOfWork.CategoryRepository.GetById(createPetDTO.CategoryId);
            if (category.ProductType != ProductType.Pet) throw new BadRequestException("Category không hợp lệ!");
            try
            {
                int productId = await _productService
                    .CreateBaseProductAsync(createPetDTO, type: 0);

                await _petService.CreatePetVariantAsync(productId, createPetDTO);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CreateToyAsync(CreateToyDTO createToyDTO)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var category = await _unitOfWork.CategoryRepository.GetById(createToyDTO.CategoryId);
            if (category.ProductType != ProductType.Toy) throw new BadRequestException("Category không hợp lệ!");
            try
            {
                var productId = await _productService
                    .CreateBaseProductAsync(createToyDTO, type: 1);
                await _toyService.CreateToyDetailAsync(productId, createToyDTO);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateFoodAsync(UpdateFoodDTO updateFoodDTO, int productId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var category = await _unitOfWork.CategoryRepository.GetById(updateFoodDTO.CategoryId);
            if (category.ProductType != ProductType.Food) throw new BadRequestException("Category không hợp lệ!");
            try
            {
                await _productService.UpdateBaseProductAsync(updateFoodDTO, productId);
                await _foodService.UpdateFoodDetailAsync(updateFoodDTO, productId);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdatePetAsync(UpdatePetDTO updatePetDTO, int productId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var category = await _unitOfWork.CategoryRepository.GetById(updatePetDTO.CategoryId);
            if (category.ProductType != ProductType.Pet) throw new BadRequestException("Category không hợp lệ!");
            try
            {
                await _productService.UpdateBaseProductAsync(updatePetDTO, productId);
                await _petService.UpdatePetVariantAsync(updatePetDTO, productId);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateToyAsync(UpdateToyDTO updateToyDTO, int productId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var category = await _unitOfWork.CategoryRepository.GetById(updateToyDTO.CategoryId);
            if (category.ProductType != ProductType.Toy) throw new BadRequestException("Category không hợp lệ!");
            try
            {
                await _productService.UpdateBaseProductAsync(updateToyDTO, productId);
                await _toyService.UpdateToyDetailAsync(updateToyDTO, productId);
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
