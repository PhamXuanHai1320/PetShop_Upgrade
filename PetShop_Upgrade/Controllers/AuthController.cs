using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShop_Upgrade.DTOS;
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
            try
            {
                var authResponse = await _authService.LoginAsync(loginDto);

                if (authResponse == null)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "Sai tên đăng nhập hoặc mật khẩu."
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Đăng nhập thành công.",
                    Data = authResponse
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex) 
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi hệ thống: " + ex.Message
                });
            }
        }
        // API đăng ký tài khoản khách hàng
        [HttpPost("register-customer")]
        [AllowAnonymous] // Cho phép bất kỳ ai cũng có thể gọi API này chưa cần đăng nhập
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterDTO registerDTO)
        {
            try
            {
                var authResponse = await _authService.RegisterAsync(registerDTO, "Customer");
                return Ok(new
                {
                    Success = true,
                    Message = "Đăng ký thành công.",
                    Data = authResponse
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        // API làm mới token khi token cũ hết hạn
        [HttpPost("refresh-token")]
        [AllowAnonymous] // Cho phép bất kỳ ai cũng có thể gọi API này chưa cần đăng nhập
        public async Task<IActionResult> RefreshToken([FromBody] TokenResponseDTO request)
        {
            try
            {
                var tokenResponse = await _authService.RefreshTokenAsync(request);
                return Ok(new
                {
                    Success = true,
                    Message = "Làm mới token thành công.",
                    Data = tokenResponse
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        // API đăng xuất (thu hồi token)
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDTO logout)
        {
            try
            {
                var memberId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                await _authService.RevokeTokenAsync(logout.RefreshToken, int.Parse(memberId));
                return Ok(new
                {
                    Success = true,
                    Message = "Thu hồi token thành công."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}
