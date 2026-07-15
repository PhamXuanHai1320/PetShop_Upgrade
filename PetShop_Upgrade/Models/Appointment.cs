using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.PENDING;
        public string? CustomerNotes { get; set; }
        public string? StaffNotes { get; set; }
        public string? CancellationReason { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? ConfirmedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public DateTimeOffset? CancelledAt { get; set; }
        public ServiceType ServiceType { get; set; } = ServiceType.PET_VIEWING;
        public int MemberId { get; set; }
        public Member Member { get; set; }
        public PetViewingAppointment PetViewingAppointment { get; set; }
    }
}
