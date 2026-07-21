using System.ComponentModel.DataAnnotations;

namespace PetShop_Upgrade.DTOS.Auth
{
    public class ExchangeGoogleCodeDTO
    {
        [Required]
        public string Code { get; set; } = string.Empty;
    }
}
