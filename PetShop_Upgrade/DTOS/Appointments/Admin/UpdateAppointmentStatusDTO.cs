using System.ComponentModel.DataAnnotations;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Appointments.Admin
{
    public class UpdateAppointmentStatusDTO
    {
        public AppointmentStatus Status { get; set; }
        public string? StaffNotes { get; set; }
        public string? Reason { get; set; }
    }
}
