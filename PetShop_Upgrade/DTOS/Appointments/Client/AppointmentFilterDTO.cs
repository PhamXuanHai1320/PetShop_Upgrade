using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Appointments.Client
{
    public class AppointmentFilterDTO
    {
        public AppointmentStatus? Status { get; set; }
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        public int? ProductId { get; set; }
    }
}
