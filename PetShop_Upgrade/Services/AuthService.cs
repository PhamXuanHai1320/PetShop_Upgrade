using Azure.Core;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PetShop_Upgrade.DTOS.Auth;
using PetShop_Upgrade.DTOS.Members.Admin;
using PetShop_Upgrade.Exceptions;
using PetShop_Upgrade.Messaging;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Options;
using PetShop_Upgrade.Repositories.Interfaces;
using PetShop_Upgrade.Services.Interfaces;
using PetShop_Upgrade.Utils;
using PetShop_Upgrade.Utils.Interfaces;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GoogleAuthOptions _option;
        private readonly int _expiresAt;
        public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService, 
            UserManager<Member> userManager, SignInManager<Member> signInManager, 
            ITokenHelper tokenHelper, IConfiguration configuration,
            IHttpClientFactory httpClientFactory, IOptions<GoogleAuthOptions> option)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenHelper = tokenHelper;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _option = option.Value;
            _expiresAt = int.Parse(_configuration["Jwt:RefreshTokenExpireDays"]);
        }
        public async Task<AuthResponseDTO> LoginAsync(LoginDTO loginDTO)
        {
            var member = await _userManager.FindByNameAsync(loginDTO.UserName);
            if (member == null)
            {
                throw new NotFoundException("User không tồn tại");
            }
            // Kiểm tra xem tài khoản có bị khóa hay không trước khi kiểm tra mật khẩu
            var isLockedOut = await _userManager.IsLockedOutAsync(member);
            if (isLockedOut)
            {
                //var lockoutEnd = await _userManager.GetLockoutEndDateAsync(member); // Lấy thời gian mở khóa
                throw new UnauthorizedException("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
            }
            var result = await _signInManager.CheckPasswordSignInAsync(member, loginDTO.Password, lockoutOnFailure: true);
            if(result.IsLockedOut)
            {
                throw new UnauthorizedException("Tài khoản của bạn đã bị khóa tạm thời. Vui lòng thử lại sau 15 phút.");
            }
            if (!result.Succeeded)
            {
                throw new UnauthorizedException("Sai tên đăng nhập hoặc mật khẩu");
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
            _unitOfWork.RefreshTokenRepository.Add(refreshTokenEntity);
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
                throw new UnauthorizedException("Token không hợp lệ");
            }
            // Tìm Member
            var memberId = principal.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (memberId == null)
            {
                throw new UnauthorizedException("Không tìm thấy MemberId trong token.");
            }
            // Kiểm tra refresh token có hợp lệ hay không
            var hashedToken = _tokenHelper.HashRefreshToken(request.RefreshToken);
            var storedToken = await _unitOfWork.RefreshTokenRepository
                .FindValidTokenOrMemberIdAsync(hashedToken, int.Parse(memberId));
            // Kiểm tra lại thông tin Member mới nhất
            var freshMember = await _unitOfWork.MemberRepository.GetById(int.Parse(memberId));
            if (freshMember == null)
            {
                throw new UnauthorizedException("Tài khoản không tồn tại hoặc đã bị khóa.");
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
                throw new UnauthorizedException("Refresh token không hợp lệ hoặc đã hết hạn.");
            }
            if(freshMember == null)
            {
                throw new NotFoundException("Tài khoản không tồn tại hoặc đã bị xóa.");
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
            _unitOfWork.RefreshTokenRepository.Add(newRefreshTokenEntity);
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
                throw new ConflictException("Tên đăng nhập đã tồn tại!");
            }
            var existingEmail = await _userManager.FindByEmailAsync(registerDTO.Email);
            if (existingEmail != null)
            {
                throw new ConflictException("Email đã được sử dụng");
            }
            // gọi Transaction để rollback nếu có lỗi
            var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
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
                    throw new BadRequestException(errorMessages);
                }
                await _userManager.AddToRoleAsync(newMember, role);

                if (role == "Customer")
                {
                    var cart = new Cart
                    {
                        MemberId = newMember.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    _unitOfWork.CartRepository.Add(cart);
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
                _unitOfWork.RefreshTokenRepository.Add(refreshTokenEntity);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                return new AuthResponseDTO
                {
                    RefreshToken = refreshToken,
                    AccessToken = token
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RevokeAllAsync(string memberId)
        {
            if (!int.TryParse(memberId, out int newmemberId))
            {
                throw new BadRequestException("Id người dùng không hợp lệ.");
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
                throw new UnauthorizedException("Bạn không có quyền hủy token này.");
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
        public async Task<AuthResponseDTO> GoogleCallbackAsync(string authorizationCode)
        {

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["code"] = authorizationCode,
                    ["client_id"] = _option.ClientId,
                    ["client_secret"] = _option.ClientSecret,
                    ["redirect_uri"] = _option.RedirectUri,
                    ["grant_type"] = "authorization_code"
                })
            };

            var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.SendAsync(request);
            var tokenResponse = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>();

            if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(tokenResponse?.IdToken))
            {
                throw new UnauthorizedException(
                    tokenResponse?.ErrorDescription ?? "Không thể đổi authorization code lấy Google token.");
            }

            return await CompleteGoogleLoginAsync(tokenResponse.IdToken, _option.ClientId);
        }
        // Hàm xử lý đăng nhập bằng Google
        private async Task<AuthResponseDTO> CompleteGoogleLoginAsync(string idToken, string clientId)
        {

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(
                    idToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { clientId }
                    });
            }
            catch (InvalidJwtException)
            {
                throw new UnauthorizedException("Google ID token không hợp lệ hoặc đã hết hạn.");
            }

            if (string.IsNullOrWhiteSpace(payload.Email) || !payload.EmailVerified)
                throw new UnauthorizedException("Tài khoản Google chưa xác minh email.");

            const string loginProvider = "Google";
            var member = await _userManager.FindByLoginAsync(loginProvider, payload.Subject);

            if (member == null)
            {
                member = await _userManager.FindByEmailAsync(payload.Email);

                if (member == null)
                {
                    await using var transaction = await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        member = new Member
                        {
                            UserName = await GenerateUniqueUserNameAsync(payload.Email),
                            Email = payload.Email,
                            EmailConfirmed = true,
                            FirstName = payload.GivenName ?? string.Empty,
                            LastName = payload.FamilyName ?? string.Empty,
                            CreateAt = DateTime.UtcNow
                        };

                        EnsureIdentitySucceeded(await _userManager.CreateAsync(member));
                        EnsureIdentitySucceeded(await _userManager.AddToRoleAsync(member, "Customer"));
                        EnsureIdentitySucceeded(await _userManager.AddLoginAsync(
                            member,
                            new UserLoginInfo(loginProvider, payload.Subject, "Google")));

                        _unitOfWork.CartRepository.Add(new Cart
                        {
                            MemberId = member.Id,
                            CreatedAt = DateTime.UtcNow
                        });
                        await _unitOfWork.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
                else
                {
                    EnsureIdentitySucceeded(await _userManager.AddLoginAsync(
                        member,
                        new UserLoginInfo(loginProvider, payload.Subject, "Google")));

                    if (!member.EmailConfirmed)
                    {
                        member.EmailConfirmed = true;
                        EnsureIdentitySucceeded(await _userManager.UpdateAsync(member));
                    }
                }
            }

            if (await _userManager.IsLockedOutAsync(member))
                throw new UnauthorizedException("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");

            var memberDTO = await MapToMemberDto(member);
            var accessToken = _jwtService.GenerateTokenAsync(memberDTO);
            var refreshToken = _tokenHelper.GenerateRefreshTokenString();

            _unitOfWork.RefreshTokenRepository.Add(new RefreshToken
            {
                HashToken = _tokenHelper.HashRefreshToken(refreshToken),
                MemberId = member.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_expiresAt),
                IsRevoked = false
            });
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
        // Hàm tạo username duy nhất dựa trên email
        private async Task<string> GenerateUniqueUserNameAsync(string email)
        {
            var baseUserName = email.Split('@')[0];
            var userName = baseUserName;
            var suffix = 1;

            while (await _userManager.FindByNameAsync(userName) != null)
                userName = $"{baseUserName}{suffix++}";

            return userName;
        }
        // Hàm kiểm tra kết quả IdentityResult và ném lỗi nếu không thành công
        private static void EnsureIdentitySucceeded(IdentityResult result)
        {
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        // Class để ánh xạ phản hồi từ Google Token API
        private sealed class GoogleTokenResponse
        {
            [JsonPropertyName("id_token")]
            public string? IdToken { get; init; }

            [JsonPropertyName("error_description")]
            public string? ErrorDescription { get; init; }
        }
    }
}
