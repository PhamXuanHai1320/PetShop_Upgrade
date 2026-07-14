using System.ComponentModel.DataAnnotations;
namespace PetShop_Upgrade.DTOS.Order.Client
{
    public class CreateOrderResponseDTO
    {
        [Required]
        public int OrderId { get; set; }
        public string? PaymentUrl { get; set; }
    }
}
