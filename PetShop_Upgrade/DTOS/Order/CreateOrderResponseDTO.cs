using System.ComponentModel.DataAnnotations;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Order
{
    public class CreateOrderResponseDTO
    {
        [Required]
        public int OrderId { get; set; }
        public string? PaymentUrl { get; set; }
    }
}
