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
            try
            {
                if (colorDTO == null || string.IsNullOrEmpty(colorDTO.ColorName))
                {
                    throw new ArgumentException("ColorDTO có giá trị null hoặc có ColorName rỗng.");
                }
                var color = new Color
                {
                    ColorName = colorDTO.ColorName
                };
                if (color == null)
                {
                    throw new ArgumentNullException(nameof(colorDTO), "Color cannot be null");
                }
                await _unitOfWork.ColorRepository.Add(color);
                await _unitOfWork.SaveChangesAsync();

                colorDTO.Id = color.Id;
                return colorDTO;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tạo color: " + ex.Message, ex);
            }
        }

        public async Task DeleteColorAsync(int id)
        {
            try
            {
                var color = await _unitOfWork.ColorRepository.GetById(id);
                if (color == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy color với id: {id}");
                }
                await _unitOfWork.ColorRepository.Delete(color);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xóa color: " + ex.Message, ex);
            }
        }
        public async Task<IEnumerable<ColorDTO>> GetAllColorsAsync()
        {
            try
            {
                var colors = await _unitOfWork.ColorRepository.GetAll();
                if (colors == null)
                {
                    throw new Exception("Không tìm thấy colors nào.");
                }
                var colorDTOs = colors.Select(c => new ColorDTO
                {
                    Id = c.Id,
                    ColorName = c.ColorName
                }).ToList();
                return colorDTOs;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy danh sách colors: " + ex.Message, ex);
            }
        }
        public async Task<ColorDTO> GetColorByIdAsync(int colorId)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy color theo id: " + ex.Message, ex);
            }
        }
        public async Task<IEnumerable<ProductColorDTO>> GetColorsByProductIdAsync(int productId)
        {
            try
            {
                var colors = await _unitOfWork.ColorRepository.GetColorsByProductId(productId);
                if (colors == null || !colors.Any())
                {
                    throw new KeyNotFoundException($"Không tìm thấy colors nào cho productId: {productId}");
                }
                var colorNames = colors.Select(c => c.ColorName).ToList();
                var productColorsDTO = colors.Select(c => new ProductColorDTO
                {
                    Id = c.Id,
                    ColorName = c.ColorName,
                    Quantity = c.ProductColors.FirstOrDefault()?.Quantity,
                    ProductId = c.ProductColors.FirstOrDefault()?.ProductId
                }).ToList();
                return productColorsDTO;


            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy colors theo productId: " + ex.Message, ex);
            }
        }
        public async Task<ColorDTO> UpdateColorAsync(int id, ColorDTO colorDTO)
        {
            try
            {
                var color = await _unitOfWork.ColorRepository.GetById(id);
                if (color == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy color với id: {id}");
                }
                color.ColorName = colorDTO.ColorName;
                _unitOfWork.ColorRepository.Update(color);
                _unitOfWork.SaveChangesAsync().Wait();
                colorDTO.Id = color.Id;
                return colorDTO;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi cập nhật color: " + ex.Message, ex);
            }
        }
    }
}
