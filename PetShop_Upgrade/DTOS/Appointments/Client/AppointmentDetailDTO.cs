using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Appointments.Client
{
    public class AppointmentDetailDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductColorName { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
        public AppointmentStatus Status { get; set; }
        public string? CustomerNotes { get; set; }
        public string? StaffNotes { get; set; }
        public string? CancellationReason { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
