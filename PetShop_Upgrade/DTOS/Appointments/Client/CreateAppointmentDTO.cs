namespace PetShop_Upgrade.DTOS.Appointments.Client
{
    public class CreateAppointmentDTO
    {
        public int ProductId { get; set; }
        public int ProductColorId { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public string? Notes { get; set; }
    }
}
