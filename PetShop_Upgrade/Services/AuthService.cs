using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.SecurityTokenService;
using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using PetShop_Upgrade.Services.Interfaces;
using PetShop_Upgrade.Utils;
using PetShop_Upgrade.Utils.Interfaces;

namespace PetShop_Upgrade.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly UserManager<Member> _userManager;
        private readonly SignInManager<Member> _signInManager;
        private readonly ITokenHelper _tokenHelper;
        private readonly IConfiguration _configuration;
        private readonly int _expiresAt;
        public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService, 
            UserManager<Member> userManager, SignInManager<Member> signInManager, 
            ITokenHelper tokenHelper, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenHelper = tokenHelper;
            _configuration = configuration;
            _expiresAt = int.Parse(_configuration["Jwt:RefreshTokenExpireDays"]);
        }
        public async Task<AuthResponseDTO> LoginAsync(LoginDTO loginDTO)
        {
            var member = await _userManager.FindByNameAsync(loginDTO.UserName);
            if (member == null)
            {
                throw new Exception("User không tồn tại");
            }
            // Kiểm tra xem tài khoản có bị khóa hay không trước khi kiểm tra mật khẩu
            var isLockedOut = await _userManager.IsLockedOutAsync(member);
            if (isLockedOut)
            {
                //var lockoutEnd = await _userManager.GetLockoutEndDateAsync(member); // Lấy thời gian mở khóa
                throw new UnauthorizedAccessException("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
            }
            var result = await _signInManager.CheckPasswordSignInAsync(member, loginDTO.Password, lockoutOnFailure: true);
            if(result.IsLockedOut)
            {
                throw new Exception("Tài khoản của bạn đã bị khóa tạm thời. Vui lòng thử lại sau 15 phút.");
            }
            if (!result.Succeeded)
            {
                throw new Exception("Sai tên đăng nhập hoặc mật khẩu");
            }
            var memberDTO = await MapToMemberDto(member);
            var token = _jwtService.GenerateTokenAsync(memberDTO);
            var refreshToken = _tokenHelper.GenerateRefreshTokenString();
            var refreshTokenEntity = new RefreshToken
            {
                HashToken = _tokenHelper.HashRefreshToken(refreshToken),
                MemberId = member.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_expiresAt),
                IsRevoked = false
            };
            await _unitOfWork.RefreshTokenRepository.Add(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDTO
            {
                RefreshToken = refreshToken,
                AccessToken = token
            };
        }

        public async Task<TokenResponseDTO> RefreshTokenAsync(TokenResponseDTO request)
        {
            // Lây thông tin từ access token đã hết hạn
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
            {
                throw new Exception("Token không hợp lệ");
            }
            // Tìm Member
            var memberId = principal.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (memberId == null)
            {
                throw new Exception("Không tìm thấy MemberId trong token.");
            }
            // Kiểm tra refresh token có hợp lệ hay không
            var hashedToken = _tokenHelper.HashRefreshToken(request.RefreshToken);
            var storedToken = await _unitOfWork.RefreshTokenRepository
                .FindValidTokenOrMemberIdAsync(hashedToken, int.Parse(memberId));
            // Kiểm tra lại thông tin Member mới nhất
            var freshMember = await _unitOfWork.MemberRepository.GetById(int.Parse(memberId));
            if (freshMember == null)
            {
                throw new UnauthorizedAccessException("Tài khoản không tồn tại hoặc đã bị khóa.");
            }
            // Tạo token mới
            var memberDTO = new MemberDTO
            {
                Id = int.Parse(memberId),
                Username = freshMember.UserName,
                Role = (await _userManager.GetRolesAsync(freshMember)).FirstOrDefault()
            };
            var newAccessToken = _jwtService.GenerateTokenAsync(memberDTO);
            
            if (storedToken is null || !storedToken.IsActive)
            {
                throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã hết hạn.");
            }
            storedToken.IsRevoked = true;
            // Tạo bản ghi lưu Refresh Token mới xuống Database
            var newRefreshToken = _tokenHelper.GenerateRefreshTokenString();
            var newRefreshTokenEntity = new RefreshToken
            {
                HashToken = _tokenHelper.HashRefreshToken(newRefreshToken),
                MemberId = int.Parse(memberId),
                ExpiresAt = DateTime.UtcNow.AddDays(_expiresAt)
            };
            await _unitOfWork.RefreshTokenRepository.Add(newRefreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();
            return new TokenResponseDTO
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDTO, string role)
        {
            var existingMember = await _userManager.FindByNameAsync(registerDTO.Username);
            if (existingMember != null)
            {
                throw new BadRequestException("Tên đăng nhập đã tồn tại!");
            }
            var existingEmail = await _userManager.FindByEmailAsync(registerDTO.Email);
            if (existingEmail != null)
            {
                throw new BadRequestException("Email đã được sử dụng");
            }
            // Tạo đối tượng Member mới
            var newMember = new Member
            {
                UserName = registerDTO.Username,
                Email = registerDTO.Email,
                FirstName = registerDTO.FirstName,
                LastName = registerDTO.LastName,
                PhoneNumber = registerDTO.PhoneNumber
            };
            var result = await _userManager.CreateAsync(newMember, registerDTO.Password);

            if (!result.Succeeded)
            {
                var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception(errorMessages);
            }
            await _userManager.AddToRoleAsync(newMember, role);

            if(role == "Customer")
            {
                var cart = new Cart
                {
                    MemberId = newMember.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.CartRepository.Add(cart);
            }
            // Tạo token ngay sau khi đăng ký thành công
            var memberDTO = await MapToMemberDto(newMember);
            var token = _jwtService.GenerateTokenAsync(memberDTO);
            var refreshToken = _tokenHelper.GenerateRefreshTokenString();
            var refreshTokenEntity = new RefreshToken
            {
                HashToken = _tokenHelper.HashRefreshToken(refreshToken),
                MemberId = newMember.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_expiresAt),
                IsRevoked = false
            };
            await _unitOfWork.RefreshTokenRepository.Add(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDTO
            {
                RefreshToken = refreshToken,
                AccessToken = token
            };
        }

        public async Task RevokeAllAsync(string memberId)
        {
            if (!int.TryParse(memberId, out int newmemberId))
            {
                throw new ArgumentException("Id người dùng không hợp lệ.");
            }

            // Gọi hàm Bulk Update ở Repository
            await _unitOfWork.RefreshTokenRepository.RevokeAllMemberTokensAsync(newmemberId);
        }

        public async Task RevokeTokenAsync(string refreshToken, int memberId)
        {
            var hashedToken = _tokenHelper.HashRefreshToken(refreshToken);
            // Lấy token trong Database
            var storedToken = await _unitOfWork.RefreshTokenRepository.FindValidTokenAsync(hashedToken);

            // không tìm thấy hoặc đã bị thu hồi
            if (storedToken == null)
            {
                return;
            }
            if (storedToken.MemberId != memberId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền hủy token này.");
            }

            // 3. Gọi hàm đánh dấu thu hồi bên Repository (thực chất là gán IsRevoked = true)
            await _unitOfWork.RefreshTokenRepository.RevokeTokenAsync(storedToken);

            // 4. Lưu sự thay đổi xuống Database (Nhờ Change Tracker của EF Core)
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<MemberDTO> MapToMemberDto(Member member)
        {
            return new MemberDTO
            {
                Id = member.Id,
                Email = member.Email,
                Username = member.UserName,
                FirstName = member.FirstName,
                LastName = member.LastName,
                PhoneNumber = member.PhoneNumber,
                Role = (await _userManager.GetRolesAsync(member)).FirstOrDefault()
            };
        }
    }
}
