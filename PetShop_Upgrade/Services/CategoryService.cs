using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.Exceptions;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<CategoryDTO> AddCategoryAsync(CategoryDTO categoryDTO)
        {
            if(categoryDTO == null)
            {
                throw new BadRequestException("CategoryDTO không hợp lệ");
            }
            var category = new Category();
            MapCategoryDTOToCategory(categoryDTO, category);
            await _unitOfWork.CategoryRepository.Add(category);
            await _unitOfWork.SaveChangesAsync();
            categoryDTO.Id = category.Id;
            return categoryDTO;
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _unitOfWork.CategoryRepository.GetCategoryByIdAsync(id);
            if (category == null)
            {
                throw new NotFoundException($"Không tìm thấy category với id: {id}");
            }

            bool hasLinkedProducts = await _unitOfWork.ProductRepository.HasProductsByCategoryIdAsync(id);
            if (hasLinkedProducts)
            {
                throw new BadRequestException($"Không thể xóa category '{category.Name}' vì vẫn còn sản phẩm đang liên kết.");
            }

            category.IsActive = 0; // Đánh dấu là không hoạt động thay vì xóa hoàn toàn
            await _unitOfWork.CategoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.CategoryRepository.GetAllCategoriesAsync();
            if (categories == null || !categories.Any())
            {
                return Enumerable.Empty<CategoryDTO>();
            }
            var categoryDTOs = categories.Select(MapCategoryToDTO).ToList();
            return categoryDTOs;
        }
   
        public async Task<CategoryDTO> GetCategoryByIdAsync(int id)
        {
            var category = await _unitOfWork.CategoryRepository.GetCategoryByIdAsync(id);
            if (category == null)
            {
                throw new NotFoundException($"Không tìm thấy category với id: {id}");
            }
            var categoryDTO = MapCategoryToDTO(category);
            return categoryDTO;
        }

        public async Task<IEnumerable<CategoryDTO>> GetCategoryByNameAsync(string Name)
        {
            var categories = await _unitOfWork.CategoryRepository.GetCategoriesByNameAsync(Name);
            if (categories == null || !categories.Any())
            {
                return Enumerable.Empty<CategoryDTO>();
            }
            var categoryDTOs = categories.Select(MapCategoryToDTO).ToList();
            return categoryDTOs;
        }

        public async Task<CategoryDTO> UpdateCategoryAsync(CategoryDTO categoryDTO)
        {
            var category = await _unitOfWork.CategoryRepository.GetById(categoryDTO.Id);
            if (category == null)
            {
                throw new NotFoundException($"Không tìm thấy category với id: {categoryDTO.Id}");
            }
            if(category.IsActive == 0)
            {
                throw new BadRequestException($"Không thể cập nhật category với id: {categoryDTO.Id} vì nó đã bị vô hiệu hóa");
            }
            MapCategoryDTOToCategory(categoryDTO, category);
            await _unitOfWork.CategoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync();
            return categoryDTO;
        }
        private void MapCategoryDTOToCategory(CategoryDTO categoryDTO, Models.Category category)
        {
            category.Name = categoryDTO.Name?.Trim();
            category.Description = categoryDTO.Description;
            category.ProductType = categoryDTO.ProductType?.Trim();
            category.IsActive = categoryDTO.IsActive;
        }
        private CategoryDTO MapCategoryToDTO(Category category)
        {
            return new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ProductType = category.ProductType,
                IsActive = category.IsActive
            };
        }
    }
}
