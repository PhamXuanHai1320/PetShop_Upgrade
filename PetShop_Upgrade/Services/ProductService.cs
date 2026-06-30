using AutoMapper;
using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.Exceptions;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using PetShop_Upgrade.Services.Interfaces;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMinioService _minioService;
        private readonly IMapper _mapper;
        public ProductService(IUnitOfWork unitOfWork, IMinioService minioService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _minioService = minioService;
            _mapper = mapper;
        }
        public async Task<int> CreateBaseProductAsync(CreateProductDTO createProductDTO, int type)
        {
            if (createProductDTO == null) throw new BadRequestException("ProductDTO không hợp lệ!");

            var uploadedImages = new List<ProductImage>();
            var uploadedFileNames = new List<string>();
            try
            {
                // Upload files to MinIO and create ProductImage objects
                if (createProductDTO.Files?.Count > 0)
                {
                    for (int i = 0; i < createProductDTO.Files.Count; i++)
                    {
                        var fileName = await _minioService.UploadFileAsync(createProductDTO.Files[i]);
                        uploadedFileNames.Add(fileName);
                        uploadedImages.Add(new ProductImage
                        {
                            ImageUrl = fileName,
                            IsMain = i == 0 ? IsMain.MAIN : IsMain.NOT_MAIN
                        });
                    }
                }
                var product = new Product
                {
                    ProductName = createProductDTO.ProductName?.Trim(),
                    Description = createProductDTO.Description,
                    ImportPrice = createProductDTO.ImportPrice,
                    SellingPrice = createProductDTO.SellingPrice,
                    Type = (ProductType)type,
                    IsActive = IsActive.ACTIVE,
                    CategoryId = createProductDTO.CategoryId,
                    ProductImages = uploadedImages,
                    ProductColors = createProductDTO.ProductColors?
                    .Select(c => new ProductColor
                    {
                        ColorId = c.ColorId,
                        Quantity = c.Quantity
                    }).ToList() ?? []
                };
                _unitOfWork.ProductRepository.Add(product);
                await _unitOfWork.SaveChangesAsync();

                return product.Id;
            }
            catch
            {
                // Nếu có lỗi xảy ra, xóa tất cả các file đã upload để tránh rác lưu trữ
                foreach (var fileName in uploadedFileNames)
                {
                    await _minioService.DeleteFileAsync(fileName);
                }
                throw;
            }
        }
        // Update product cùng function add product history, function image management, and function color synchronization
        public async Task UpdateBaseProductAsync(UpdateProductDTO updateProductDTO, int productId)
        {
            if (updateProductDTO == null)
                throw new BadRequestException("UpdateProductDTO không hợp lệ!");
            var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(productId);
            if (product == null)
                throw new NotFoundException($"Không tìm thấy sản phẩm với Id: {productId}");
            var uploadedFileNamesToRollback = new List<string>();
            var deletedFileNamesToCommit = new List<string>();
            try
            {
                // Chuyển dữ liệu sang ProductHistory trước khi update
                await HandleProductHistoryAsync(product, updateProductDTO);

                // Cập nhật field thường
                product.ProductName = updateProductDTO.ProductName?.Trim();
                product.Description = updateProductDTO.Description;
                product.ImportPrice = updateProductDTO.ImportPrice;
                product.SellingPrice = updateProductDTO.SellingPrice;
                product.CategoryId = updateProductDTO.CategoryId;

                await UpdateProductImagesAsync(product, updateProductDTO, uploadedFileNamesToRollback, deletedFileNamesToCommit);
                SyncProductColors(product, updateProductDTO);

                // Lưu DB
                _unitOfWork.ProductRepository.Update(product);
                await _unitOfWork.SaveChangesAsync();

                // DB commit thành công mới xóa file thật trên MinIO
                foreach (var fileName in deletedFileNamesToCommit)
                    await _minioService.DeleteFileAsync(fileName);
            }
            catch
            {
                foreach (var fileName in uploadedFileNamesToRollback)
                    await _minioService.DeleteFileAsync(fileName);
                throw;
            }
        }
        // function add product history
        private async Task HandleProductHistoryAsync(Product product, UpdateProductDTO updateProductDTO)
        {
            if (product.ProductName == updateProductDTO.ProductName?.Trim() &&
                product.ImportPrice == updateProductDTO.ImportPrice &&
                product.SellingPrice == updateProductDTO.SellingPrice)
                return;

            var productHistory = new ProductHistory
            {
                ProductId = product.Id,
                ProductName = product.ProductName,
                Description = product.Description,
                ImportPrice = product.ImportPrice,
                SellingPrice = product.SellingPrice,
                Version = await _unitOfWork.ProductHistoryRepository.GetLatestVersionAsync(product.Id) + 1,
                MemberId = updateProductDTO.MemberId
            };
            _unitOfWork.ProductHistoryRepository.Add(productHistory);
        }
        // function image management
        private async Task UpdateProductImagesAsync(
            Product product,
            UpdateProductDTO updateProductDTO,
            List<string> uploadedFileNamesToRollback,
            List<string> deletedFileNamesToCommit)
        {
            // Xóa ảnh cũ theo DeletedImageIds
            if (updateProductDTO.DeletedImageIds?.Count > 0)
            {
                var imagesToRemove = product.ProductImages
                    .Where(img => updateProductDTO.DeletedImageIds.Contains(img.Id))
                    .ToList();

                foreach (var img in imagesToRemove)
                {
                    deletedFileNamesToCommit.Add(img.ImageUrl);
                    product.ProductImages.Remove(img);
                }
            }

            // Snapshot ảnh cũ còn lại trước khi thêm mới
            var remainingOldImages = product.ProductImages
                .OrderBy(img => img.Id)
                .ToList();

            // Upload ảnh mới
            var newImageEntities = new List<ProductImage>();
            if (updateProductDTO.NewImages?.Count > 0)
            {
                foreach (var file in updateProductDTO.NewImages)
                {
                    var fileName = await _minioService.UploadFileAsync(file);
                    uploadedFileNamesToRollback.Add(fileName);

                    var newImg = new ProductImage { ImageUrl = fileName, IsMain = 0 };
                    newImageEntities.Add(newImg);
                    product.ProductImages.Add(newImg);
                }
            }

            // Reset toàn bộ IsMain
            foreach (var img in product.ProductImages)
                img.IsMain = 0;

            // Xác định ảnh Main theo index
            var orderedAllImages = remainingOldImages.Concat(newImageEntities).ToList();
            if (orderedAllImages.Count > 0)
            {
                var safeIndex = updateProductDTO.MainImageIndex;
                if (safeIndex < 0 || safeIndex >= orderedAllImages.Count)
                    safeIndex = 0;

                orderedAllImages[safeIndex].IsMain = IsMain.MAIN;
            }
        }
        // function color synchronization
        private void SyncProductColors(Product product, UpdateProductDTO updateProductDTO)
        {
            if (updateProductDTO.ProductColors == null) return;

            var incomingIds = updateProductDTO.ProductColors
                .Where(c => c.Id != 0)
                .Select(c => c.Id)
                .ToList();

            // Xóa màu không còn trong danh sách mới
            var colorsToRemove = product.ProductColors
                .Where(c => !incomingIds.Contains(c.Id))
                .ToList();
            foreach (var c in colorsToRemove)
                product.ProductColors.Remove(c);

            foreach (var colorDto in updateProductDTO.ProductColors)
            {
                if (colorDto.Id != 0)
                {
                    // Update màu cũ
                    var existing = product.ProductColors.FirstOrDefault(c => c.Id == colorDto.Id);
                    if (existing != null)
                    {
                        existing.ColorId = colorDto.ColorId;
                        existing.Quantity = colorDto.Quantity;
                    }
                }
                else
                {
                    // Thêm màu mới
                    product.ProductColors.Add(new ProductColor
                    {
                        ColorId = colorDto.ColorId,
                        Quantity = colorDto.Quantity
                    });
                }
            }
        }
        // Function to GET Product Detail by Id and function MapPet, MapToy, MapFood
        public async Task<ProductDetailDTO> GetProductDetailByIdAsync(int productId)
        {
            var product = await _unitOfWork.ProductRepository.GetProductDetailsByIdAsync(productId);
            if (product == null)
                throw new NotFoundException($"Không tìm thấy sản phẩm với Id: {productId}");

            ProductDetailDTO productDetailDTO = product.Type switch
            {
                ProductType.Pet => MapPetDetail(product),
                ProductType.Toy => MapToyDetail(product),
                ProductType.Food => MapFoodDetail(product),
                _ => throw new Exception("Loại sản phẩm không hợp lệ")
            };
            productDetailDTO.FinalPrice = await CalculateFinalPrice(product);
            return productDetailDTO;
        }

        private async Task<double> CalculateFinalPrice(Product product)
        {
            var discounts = await _unitOfWork.DiscountRepository
                .GetDiscountsByProductIdAndCategoryIdAsync(product.Id, product.CategoryId);

            var eligibleDiscounts = discounts.Where(d =>
                !d.MinOrderValue.HasValue || d.MinOrderValue <= product.SellingPrice);

            double finalPrice = product.SellingPrice;

            foreach (var discount in eligibleDiscounts)
            {
                double discountedPrice;
                if (discount.DiscountType == 0) // Percentage
                {
                    var amount = product.SellingPrice * discount.DiscountValue / 100.0;
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

            return Math.Max(finalPrice, 0); // Không để giá âm
        }

        private ProductDetailDTO MapPetDetail(Product product)
        {
            var petDetailDTO = new PetDetailDTO();
            _mapper.Map(product, petDetailDTO);
            petDetailDTO.Gender = product.PetVariant?.Gender ?? "";
            petDetailDTO.Size = product.PetVariant?.Size ?? "";
            petDetailDTO.Weight = product.PetVariant?.Weight ?? 0;
            return petDetailDTO;
        }
        private ProductDetailDTO MapToyDetail(Product product)
        {
            var toyDetailDTO = new ToyDetailDTO();
            _mapper.Map(product, toyDetailDTO);
            toyDetailDTO.Material = product.ToysDetails?.Material ?? "";
            toyDetailDTO.Size = product.ToysDetails?.Size ?? "";
            return toyDetailDTO;
        }
        private ProductDetailDTO MapFoodDetail(Product product)
        {
            var foodDetailDTO = new FoodDetailDTO();
            _mapper.Map(product, foodDetailDTO);
            foodDetailDTO.Flavor = product.FoodsDetails?.Flavor ?? "";
            foodDetailDTO.WeightGram = product.FoodsDetails?.WeightGram ?? 0;
            foodDetailDTO.ExprireDate = product.FoodsDetails?.ExprireDate;
            foodDetailDTO.AgeGroup = product.FoodsDetails?.AgeGroup;
            return foodDetailDTO;
        }
        // Function to GET All Product and By Filler
        public async Task<IEnumerable<ProductItemsDTO>> GetProductByFilterAsync(
            ProductFilterDTO productFilter, int page, int pageSize)
        {
            var products = await _unitOfWork.ProductRepository.GetProductByFilterAsync(productFilter, page, pageSize);
            if (!products.Any())
                return Enumerable.Empty<ProductItemsDTO>();

            var productIds = products.Select(p => p.Id).ToList();
            var categoryIds = products.Select(p => p.CategoryId).Distinct().ToList();

            var allDiscounts = (await _unitOfWork.DiscountRepository
                .GetDiscountsByProductIdAndCategoryIdAsync(productIds, categoryIds))
                .ToList();

            var result = new List<ProductItemsDTO>();

            foreach (var product in products)
            {
                var applicableDiscounts = allDiscounts.Where(d =>
                    d.DiscountProducts.Any(dp => dp.ProductId == product.Id) ||
                    d.DiscountCategories.Any(dc => dc.CategoryId == product.CategoryId));

                var dto = _mapper.Map<ProductItemsDTO>(product);
                dto.FinalPrice = CalculateFinalPriceFromDiscounts(product, applicableDiscounts);

                result.Add(dto);
            }

            return result;
        }

        private double CalculateFinalPriceFromDiscounts(Product product, IEnumerable<Discount> discounts)
        {
            var eligible = discounts.Where(d =>
                !d.MinOrderValue.HasValue || d.MinOrderValue <= product.SellingPrice);

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
        // DELETE product
        public async Task DeleteProductAsync(int productId)
        {
            var product = await _unitOfWork.ProductRepository.GetById(productId);
            if (product == null)
                throw new NotFoundException($"Không tìm thấy sản phẩm với Id: {productId}");
            if (product.IsActive == 0)
                throw new BadRequestException("Sản phẩm đã bị ẩn từ trước");

            product.IsActive = 0;

            var cartItem = await _unitOfWork.CartItemRepository.FindAsync(c => c.ProductId == productId);
            foreach (var item in cartItem)
            {
                _unitOfWork.CartItemRepository.Delete(item);
            }
            // Giải phóng InventoryLock nếu có

            _unitOfWork.ProductRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
