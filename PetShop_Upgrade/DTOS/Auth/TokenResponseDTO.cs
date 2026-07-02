namespace PetShop_Upgrade.DTOS.Auth
{
    public class TokenResponseDTO
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime AccessTokenExpiry { get; set; }
    }
}
