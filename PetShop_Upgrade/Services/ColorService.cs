using PetShop_Upgrade.DTOS;
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
                throw new ArgumentException("ColorDTO không hợp lệ hoặc ColorName rỗng.");
            }

            var color = new Color
            {
                ColorName = colorDTO.ColorName.Trim()
            };

            await _unitOfWork.ColorRepository.Add(color);
            await _unitOfWork.SaveChangesAsync();

            colorDTO.Id = color.Id;
            return colorDTO;
        }

        public async Task DeleteColorAsync(int id)
        {
            var color = await _unitOfWork.ColorRepository.GetById(id);
            if (color == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy color với id: {id}");
            }

            await _unitOfWork.ColorRepository.Delete(color);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<IEnumerable<ColorDTO>> GetAllColorsAsync()
        {
            var colors = await _unitOfWork.ColorRepository.GetAll();

            if (colors == null || !colors.Any())
            {
                return Enumerable.Empty<ColorDTO>();
            }    
            var colorDTOs = colors.Select(c => new ColorDTO
            {
                Id = c.Id,
                ColorName = c.ColorName
            }).ToList();
            return colorDTOs;
        }
        public async Task<ColorDTO> GetColorByIdAsync(int colorId)
        {
            var color = await _unitOfWork.ColorRepository.GetById(colorId);
            if (color == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy color với id: {colorId}");
            }

            var colorDTO = new ColorDTO
            {
                Id = color.Id,
                ColorName = color.ColorName
            };
            return colorDTO;
        }

        public async Task<IEnumerable<ColorDTO>> GetColorsByName(string colorName)
        {
            var colors = await _unitOfWork.ColorRepository.GetColorsByName(colorName);

            if (colors == null || !colors.Any())
            {
                return Enumerable.Empty<ColorDTO>();
            }    

            var colorDTOs = colors.Select(c => new ColorDTO
            {
                Id = c.Id,
                ColorName = c.ColorName
            }).ToList();
            return colorDTOs;
        }
        public async Task<IEnumerable<ProductColorDTO>> GetColorsByProductIdAsync(int productId)
        {
            var colors = await _unitOfWork.ColorRepository.GetColorsByProductId(productId);
            if (colors == null || !colors.Any())
            {
                return Enumerable.Empty<ProductColorDTO>();
            }    

            var productColorsDTO = colors.Select(c => new ProductColorDTO
            {
                Id = c.Id,
                ColorName = c.ColorName,
                Quantity = c.ProductColors.FirstOrDefault()?.Quantity,
                ProductId = c.ProductColors.FirstOrDefault()?.ProductId
            }).ToList();
            return productColorsDTO;
        }
        public async Task<ColorDTO> UpdateColorAsync(int id, ColorDTO colorDTO)
        {
            var color = await _unitOfWork.ColorRepository.GetById(id);
            if (color == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy color với id: {id}");
            }

            color.ColorName = colorDTO.ColorName;

            await _unitOfWork.ColorRepository.Update(color);
            await _unitOfWork.SaveChangesAsync();

            colorDTO.Id = color.Id;
            return colorDTO;
        }
    }
}
