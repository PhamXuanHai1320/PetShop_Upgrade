namespace PetShop_Upgrade.Models
{
    public class PetViewingAppointment
    {
        public int AppointmentId { get; set; }  // PK + FK
        public Appointment Appointment { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int ProductColorId { get; set; }
    }
}
