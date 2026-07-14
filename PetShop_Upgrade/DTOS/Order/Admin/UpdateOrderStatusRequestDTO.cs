using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Order.Admin
{
    public class UpdateOrderStatusRequestDTO
    {
        public OrderStatus NewStatus { get; set; }  
    }
}
