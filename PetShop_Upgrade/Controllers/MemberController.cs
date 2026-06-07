using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Controllers
{
    public class MemberController : ControllerBase
    {
        private readonly IAuthService _authService;
        public MemberController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("logout-all-devices")]
        [Authorize]
        public async Task<IActionResult> LogoutAllDevices()
        {
            try
            {
                var memberId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                if (memberId == null)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin người dùng."
                    });
                }
                // Gọi service để thu hồi tất cả refresh token của người dùng
                await _authService.RevokeAllAsync(memberId);
                return Ok(new
                {
                    Success = true,
                    Message = "Đăng xuất khỏi tất cả thiết bị thành công."
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
