using PetShop_Upgrade.DTOS.Auth;

namespace PetShop_Upgrade.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDTO, string role);
        Task<AuthResponseDTO> LoginAsync(LoginDTO loginDTO);
        Task<TokenResponseDTO> RefreshTokenAsync(TokenResponseDTO request);
        Task RevokeTokenAsync(string refreshToken, int memberId);
        Task RevokeAllAsync(string memberId);
    }
}
