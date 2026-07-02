using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShop_Upgrade.DTOS.Auth;
using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        // API đăng nhập
        [HttpPost("login")]
        [AllowAnonymous] // Cho phép bất kỳ ai cũng có thể gọi API này chưa cần đăng nhập
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
                var authResponse = await _authService.LoginAsync(loginDto);
                return Ok(new
                {
                    Success = true,
                    Message = "Đăng nhập thành công.",
                    Data = authResponse
                });
        }
        // API đăng ký tài khoản khách hàng
        [HttpPost("register-customer")]
        [AllowAnonymous] // Cho phép bất kỳ ai cũng có thể gọi API này chưa cần đăng nhập
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterDTO registerDTO)
        {
            var authResponse = await _authService.RegisterAsync(registerDTO, "Customer");
            return Ok(new
            {
                Success = true,
                Message = "Đăng ký thành công.",
                Data = authResponse
            });
        }
        // API làm mới token khi token cũ hết hạn
        [HttpPost("refresh-token")]
        [AllowAnonymous] // Cho phép bất kỳ ai cũng có thể gọi API này chưa cần đăng nhập
        public async Task<IActionResult> RefreshToken([FromBody] TokenResponseDTO request)
        {
            var tokenResponse = await _authService.RefreshTokenAsync(request);
            return Ok(new
            {
                Success = true,
                Message = "Làm mới token thành công.",
                Data = tokenResponse
            });
        }
        // API đăng xuất (thu hồi token)
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDTO logout)
        {
            var memberIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out int memberId))
                return Unauthorized(new { Success = false, Message = "Token không hợp lệ." });

            await _authService.RevokeTokenAsync(logout.RefreshToken, memberId);
            return Ok(new
            {
                Success = true,
                Message = "Thu hồi token thành công."
            });
        }
    }
}
