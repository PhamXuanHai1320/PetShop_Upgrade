using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.PENDING; // PENDING/CONFIRMED/COMPLETED/CANCELLED
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public ServiceType ServiceType { get; set; } = ServiceType.PET_VIEWING;// 0: PET_VIEWING
        public int MemberId { get; set; }
        public Member Member { get; set; }
    }
}
