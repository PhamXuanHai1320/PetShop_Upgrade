using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Appointments.Admin
{
    public class AdminAppointmentItemDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int MemberId { get; set; }
        public string MemberName { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
        public AppointmentStatus Status { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
