using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Order
{
    public class CreateOrderResultDTO
    {
        public int OrderId { get; set; }
        public decimal FinalPrice { get; set; }
    }
}
