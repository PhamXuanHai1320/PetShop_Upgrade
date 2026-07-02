using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShop_Upgrade.DTOS.Auth;
using PetShop_Upgrade.Services.Interfaces;
namespace PetShop_Upgrade.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IMemberService _memberService;

        public AdminController(IAuthService authService, IMemberService memberService)
        {
            _authService = authService;
            _memberService = memberService;
        }
        // Admin có thể tạo tài khoản nhân viên
        [HttpPost("register-employee")]
        public async Task<IActionResult> CreateStaff(RegisterDTO registerDTO)
        {
            var authResponse = await _authService.RegisterAsync(registerDTO, "Employee");
            return Ok(new
            {
                Success = true,
                Message = "Tạo tài khoản nhân viên thành công."
            });
        }
        // Admin có thể tạo tài khoản quản trị viên khác
        [HttpPost("register-admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] RegisterDTO registerDTO)
        {
            var authResponse = await _authService.RegisterAsync(registerDTO, "Admin");
            return Ok(new
            {
                Success = true,
                Message = "Tạo tài khoản quản trị viên thành công."
            });
        }
        // Admin có thể khóa tài khoản Member bằng cách gọi API này
        [HttpPost("block-member/{memberId}")]
        public async Task<IActionResult> BlockAndRevokeMember([FromRoute] string memberId)
        {
            await _memberService.BlockMemberAsync(memberId);
            return Ok(new
            {
                Success = true,
                Message = "Đã khóa tài khoản thành công."
            });
        }
    }
}
