using System.ComponentModel.DataAnnotations;

namespace PetShop_Upgrade.DTOS
{
    public class LoginDTO
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
