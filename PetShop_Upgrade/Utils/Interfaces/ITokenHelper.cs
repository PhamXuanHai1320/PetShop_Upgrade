namespace PetShop_Upgrade.Utils.Interfaces
{
    public interface ITokenHelper
    {
        string GenerateRefreshTokenString();
        string HashRefreshToken(string plainTextToken);

    }
}
