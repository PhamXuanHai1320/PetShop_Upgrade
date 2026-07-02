using AutoMapper;
using PetShop_Upgrade.DTOS.Cart;
using PetShop_Upgrade.Exceptions;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories;
using PetShop_Upgrade.Repositories.Interfaces;
using PetShop_Upgrade.Services.Interfaces;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork; // nếu không có UoW, thay bằng AppDbContext để SaveChangesAsync
        private readonly IMapper _mapper;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CartDTO> GetCartAsync(int memberId)
        {
            var cart = await GetOrCreateCartAsync(memberId);
            var cartDTO = _mapper.Map<CartDTO>(cart);
            await FillColorNamesAsync(cartDTO);
            return cartDTO;
        }

        public async Task<CartDTO> AddToCartAsync(int memberId, AddCartItemRequestDTO addCartItemRequestDTO)
        {
            if (addCartItemRequestDTO.Quantity <= 0)
                throw new BadRequestException("Số lượng sản phẩm phải lớn hơn 0.");

            var cart = await GetOrCreateCartAsync(memberId);

            // Lấy ProductColor để check tồn kho
            var productColor = await _unitOfWork.ProductColorRepository
                .GetById(addCartItemRequestDTO.ProductColorId);
            if (productColor is null)
                throw new NotFoundException("Không tìm thấy màu sản phẩm tương ứng.");

            var product = await _unitOfWork.ProductRepository
                .GetById(addCartItemRequestDTO.ProductId);
            if (product.IsActive != IsActive.ACTIVE)
                throw new BadRequestException("Sản phẩm hiện không khả dụng.");
            if(product.Type == ProductType.Pet)
                throw new BadRequestException("Không thể thêm thú cưng vào giỏ hàng. Vui lòng đặt lịch hẹn để mua thú cưng.");

            var existingItem = await _unitOfWork.CartRepository
                .GetCartItemAsync(cart.Id, addCartItemRequestDTO.ProductColorId);
            var newQuantity = (existingItem?.Quantity ?? 0) + addCartItemRequestDTO.Quantity;

            if (newQuantity > productColor.Quantity)
                throw new BadRequestException($"Chỉ còn {productColor.Quantity} sản phẩm trong kho.");

            if (existingItem is not null)
            {
                existingItem.Quantity = newQuantity;
                existingItem.Price = product.SellingPrice;
                _unitOfWork.CartRepository.UpdateCartItem(existingItem);
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = addCartItemRequestDTO.ProductId,
                    ProductColorId = addCartItemRequestDTO.ProductColorId,
                    Quantity = addCartItemRequestDTO.Quantity,
                    Price = product.SellingPrice
                };
                await _unitOfWork.CartRepository.AddCartItemAsync(newItem);
            }

            RecalculateTotal(cart);
            await _unitOfWork.SaveChangesAsync();

            var updatedCart = await _unitOfWork.CartRepository.GetCartByMemberIdAsync(memberId);
            var cartDTO = _mapper.Map<CartDTO>(updatedCart);
            await FillColorNamesAsync(cartDTO);
            return cartDTO;
        }

        public async Task<CartDTO> UpdateCartItemAsync(int memberId, int cartItemId, UpdateCartItemRequestDTO request)
        {
            if (request.Quantity <= 0)
                throw new BadRequestException("Số lượng sản phẩm phải lớn hơn 0.");

            var cartItem = await _unitOfWork.CartRepository.GetCartItemByIdAsync(cartItemId)
                ?? throw new NotFoundException("Không tìm thấy sản phẩm trong giỏ hàng.");

            if (cartItem.Cart.MemberId != memberId)
                throw new BadRequestException("Bạn không có quyền thao tác trên giỏ hàng này.");

            var productColor = await _unitOfWork.ProductColorRepository
                .GetById(cartItem.ProductColorId);

            if (request.Quantity > productColor.Quantity)
                throw new BadRequestException($"Chỉ còn {productColor.Quantity} sản phẩm trong kho.");

            cartItem.Quantity = request.Quantity;
            _unitOfWork.CartRepository.UpdateCartItem(cartItem);

            var cart = await _unitOfWork.CartRepository.GetCartByMemberIdAsync(memberId);
            RecalculateTotal(cart!);
            await _unitOfWork.SaveChangesAsync();

            var cartDTO = _mapper.Map<CartDTO>(cart);
            await FillColorNamesAsync(cartDTO);
            return cartDTO;
        }

        public async Task<CartDTO> RemoveCartItemAsync(int memberId, int cartItemId)
        {
            var cartItem = await _unitOfWork.CartRepository.GetCartItemByIdAsync(cartItemId)
                ?? throw new NotFoundException("Không tìm thấy sản phẩm trong giỏ hàng.");

            if (cartItem.Cart.MemberId != memberId)
                throw new BadRequestException("Bạn không có quyền thao tác trên giỏ hàng này.");

            cartItem.Cart.TotalPrice -= (cartItem.Price * cartItem.Quantity);
            _unitOfWork.CartRepository.RemoveCartItem(cartItem);

            await _unitOfWork.SaveChangesAsync();

            var updatedCart = await _unitOfWork.CartRepository.GetCartByMemberIdAsync(memberId);
            var cartDTO = _mapper.Map<CartDTO>(updatedCart);
            await FillColorNamesAsync(cartDTO);
            return cartDTO;
        }

        public async Task ClearCartAsync(int memberId)
        {
            var cart = await _unitOfWork.CartRepository.GetCartByMemberIdAsync(memberId);
            if (cart is null || cart.CartItems.Count == 0) return;

            _unitOfWork.CartRepository.RemoveCartItems(cart.CartItems);
            cart.TotalPrice = 0;

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<Cart> GetOrCreateCartAsync(int memberId)
        {
            var cart = await _unitOfWork.CartRepository.GetCartByMemberIdAsync(memberId);
            if (cart is not null) return cart;

            cart = await _unitOfWork.CartRepository.CreateCartAsync(memberId);
            await _unitOfWork.SaveChangesAsync();
            return cart;
        }

        private static void RecalculateTotal(Cart cart)
        {
            cart.TotalPrice = cart.CartItems.Sum(ci => ci.Price * ci.Quantity);
        }
        private async Task FillColorNamesAsync(CartDTO cartDTO)
        {
            if (cartDTO.CartItems.Count == 0) return;

            var colorIds = cartDTO.CartItems.Select(ci => ci.ProductColorId);
            var colorMap = await _unitOfWork.CartRepository.GetColorNamesByProductColorIdsAsync(colorIds);

            foreach (var item in cartDTO.CartItems)
            {
                if (colorMap.TryGetValue(item.ProductColorId, out var colorName))
                    item.ColorName = colorName;
            }
        }
    }
}
