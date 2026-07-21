using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using PetShop_Upgrade.DTOS.Auth;
using PetShop_Upgrade.Services.Interfaces;
using PetShop_Upgrade.Options;
using Microsoft.Extensions.Options;
using PetShop_Upgrade.Messaging;

namespace PetShop_Upgrade.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private static readonly object ExchangeCodeLock = new();
        private readonly IAuthService _authService;
        private readonly IMemoryCache _cache;
        private readonly GoogleAuthOptions _option;
        private readonly IConfiguration _configuration;

        public AuthController(
            IAuthService authService,
            IOptions<GoogleAuthOptions> option,
            IMemoryCache cache,
            IConfiguration configuration)
        {
            _authService = authService;
            _option = option.Value;
            _cache = cache;
            _configuration = configuration;
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

        // Bắt đầu OAuth Authorization Code flow và chuyển người dùng sang Google.
        [HttpGet("google")]
        public IActionResult Google([FromQuery] string returnUrl)
        {
            var clientId = _option.ClientId;
            var redirectUri = _option.RedirectUri;

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(redirectUri))
                throw new InvalidOperationException("Chưa cấu hình GoogleGmail:ClientId hoặc GoogleGmail:RedirectUri.");

            if (!IsAllowedReturnUrl(returnUrl))
                return BadRequest(new { Success = false, Message = "returnUrl không được phép." });

            var state = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            _cache.Set($"google-state:{state}", returnUrl, TimeSpan.FromMinutes(10));

            Response.Cookies.Append("google_oauth_state", state, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/api/auth/callback",
                MaxAge = TimeSpan.FromMinutes(10),
                IsEssential = true
            });

            var authorizationUrl = QueryHelpers.AddQueryString(
                "https://accounts.google.com/o/oauth2/v2/auth",
                new Dictionary<string, string?>
                {
                    ["client_id"] = clientId,
                    ["redirect_uri"] = redirectUri,
                    ["response_type"] = "code",
                    ["scope"] = "openid email profile",
                    ["state"] = state,
                    ["prompt"] = "select_account"
                });

            return Redirect(authorizationUrl);
        }

        // Google chuyển trình duyệt về đây sau khi người dùng xác thực thành công.
        [HttpGet("callback")]
        public async Task<IActionResult> GoogleCallback(
            [FromQuery] string? code,
            [FromQuery] string? state,
            [FromQuery] string? error)
        {
            if (!string.IsNullOrWhiteSpace(error))
                return BadRequest(new { Success = false, Message = $"Google từ chối đăng nhập: {error}." });

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
                return BadRequest(new { Success = false, Message = "Google callback thiếu code hoặc state." });

            if (!Request.Cookies.TryGetValue("google_oauth_state", out var savedState)
                || !CryptographicOperations.FixedTimeEquals(
                    System.Text.Encoding.UTF8.GetBytes(savedState),
                    System.Text.Encoding.UTF8.GetBytes(state)))
            {
                return BadRequest(new { Success = false, Message = "OAuth state không hợp lệ hoặc đã hết hạn." });
            }

            if (!_cache.TryGetValue($"google-state:{state}", out string? returnUrl)
                || string.IsNullOrWhiteSpace(returnUrl))
            {
                return BadRequest(new { Success = false, Message = "Phiên đăng nhập Google đã hết hạn." });
            }

            _cache.Remove($"google-state:{state}");

            Response.Cookies.Delete("google_oauth_state", new CookieOptions
            {
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/api/auth/callback"
            });

            var authResponse = await _authService.GoogleCallbackAsync(code);
            var exchangeCode = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            _cache.Set(
                $"google-exchange:{exchangeCode}",
                authResponse,
                TimeSpan.FromMinutes(2));

            return Redirect(QueryHelpers.AddQueryString(returnUrl, "code", exchangeCode));
        }

        // Frontend đổi mã dùng một lần lấy JWT của hệ thống.
        [HttpPost("exchange-google-code")]
        public IActionResult ExchangeGoogleCode([FromBody] ExchangeGoogleCodeDTO request)
        {
            var cacheKey = $"google-exchange:{request.Code}";
            AuthResponseDTO? authResponse;

            lock (ExchangeCodeLock)
            {
                if (!_cache.TryGetValue(cacheKey, out authResponse) || authResponse == null)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "Mã đăng nhập không hợp lệ, đã hết hạn hoặc đã được sử dụng."
                    });
                }

                _cache.Remove(cacheKey);
            }

            Response.Headers.CacheControl = "no-store";

            return Ok(new
            {
                Success = true,
                Message = "Đăng nhập bằng Google thành công.",
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
        private bool IsAllowedReturnUrl(string returnUrl)
        {
            if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var returnUri)
                || (returnUri.Scheme != Uri.UriSchemeHttp && returnUri.Scheme != Uri.UriSchemeHttps)
                || !string.IsNullOrEmpty(returnUri.UserInfo)
                || !string.IsNullOrEmpty(returnUri.Fragment))
            {
                return false;
            }

            var requestedOrigin = returnUri.GetLeftPart(UriPartial.Authority);
            var allowedOrigins = _configuration
                .GetSection("Frontend:AllowedOrigins")
                .Get<string[]>() ?? [];

            return allowedOrigins.Any(origin =>
                Uri.TryCreate(origin, UriKind.Absolute, out var allowedUri)
                && string.Equals(
                    requestedOrigin,
                    allowedUri.GetLeftPart(UriPartial.Authority),
                    StringComparison.OrdinalIgnoreCase));
        }
    }
}
