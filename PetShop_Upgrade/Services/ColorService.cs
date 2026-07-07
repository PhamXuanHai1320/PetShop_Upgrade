using PetShop_Upgrade.DTOS.Colors;
using PetShop_Upgrade.Exceptions;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Services
{
    public class ColorService : IColorService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ColorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ColorDTO> CreateColorAsync(ColorDTO colorDTO)
        {
            if (colorDTO == null || string.IsNullOrEmpty(colorDTO.ColorName))
            {
                throw new BadRequestException("ColorDTO không hợp lệ hoặc ColorName rỗng.");
            }

            var color = new Color
            {
                ColorName = colorDTO.ColorName.Trim()
            };

            _unitOfWork.ColorRepository.Add(color);
            await _unitOfWork.SaveChangesAsync();

            colorDTO.Id = color.Id;
            return colorDTO;
        }

        public async Task DeleteColorAsync(int id)
        {
            var color = await _unitOfWork.ColorRepository.GetColorByIdAsync(id);
            if (color == null)
            {
                throw new NotFoundException($"Không tìm thấy color với id: {id}");
            }
            
            var hasProductColor = await _unitOfWork.ColorRepository.HasProductColorByColorIdAsync(id);
            if(hasProductColor) {
                throw new BadRequestException($"Không thể xóa color '{color.ColorName}' vì vẫn còn sản phẩm đang liên kết.");
            }

            color.IsActive = 0; // Đánh dấu là không hoạt động thay vì xóa hoàn toàn
            _unitOfWork.ColorRepository.Update(color);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<IEnumerable<ColorDTO>> GetAllColorsAsync()
        {
            var colors = await _unitOfWork.ColorRepository.GetAllColorsAsync();

            if (colors == null || !colors.Any())
            {
                return Enumerable.Empty<ColorDTO>();
            }    
            var colorDTOs = colors.Select(c => new ColorDTO
            {
                Id = c.Id,
                ColorName = c.ColorName,
                IsActive = c.IsActive
            }).ToList();
            return colorDTOs;
        }
        public async Task<ColorDTO> GetColorByIdAsync(int colorId)
        {
            var color = await _unitOfWork.ColorRepository.GetColorByIdAsync(colorId);
            if (color == null)
            {
                throw new NotFoundException($"Không tìm thấy color với id: {colorId}");
            }

            var colorDTO = new ColorDTO
            {
                Id = color.Id,
                ColorName = color.ColorName,
                IsActive = color.IsActive
            };
            return colorDTO;
        }

        public async Task<IEnumerable<ColorDTO>> GetColorsByName(string colorName)
        {
            var colors = await _unitOfWork.ColorRepository.GetColorsByNameAsync(colorName);

            if (colors == null || !colors.Any())
            {
                return Enumerable.Empty<ColorDTO>();
            }    

            var colorDTOs = colors.Select(c => new ColorDTO
            {
                Id = c.Id,
                ColorName = c.ColorName,
                IsActive = c.IsActive
            }).ToList();
            return colorDTOs;
        }
        public async Task<IEnumerable<ProductColorRequestDTO>> GetColorsByProductIdAsync(int productId)
        {
            var colors = await _unitOfWork.ColorRepository.GetColorsByProductIdAsync(productId);
            if (colors == null || !colors.Any())
            {
                return Enumerable.Empty<ProductColorRequestDTO>();
            }

            var productColorsDTO = colors.Select(c =>
            {
                var pc = c.ProductColors.FirstOrDefault();
                return new ProductColorRequestDTO
                {
                    Id = c.Id,
                    ColorName = c.ColorName,
                    Quantity = pc.Quantity,
                    ProductId = pc.ProductId
                };
            }).ToList();
            return productColorsDTO;
        }
        public async Task<ColorDTO> UpdateColorAsync(int id, ColorDTO colorDTO)
        {
            var color = await _unitOfWork.ColorRepository.GetById(id);
            if (color == null)
            {
                throw new NotFoundException($"Không tìm thấy color với id: {id}");
            }

            color.ColorName = colorDTO.ColorName?.Trim();

            _unitOfWork.ColorRepository.Update(color);
            await _unitOfWork.SaveChangesAsync();

            colorDTO.Id = color.Id;
            return colorDTO;
        }
    }
}
