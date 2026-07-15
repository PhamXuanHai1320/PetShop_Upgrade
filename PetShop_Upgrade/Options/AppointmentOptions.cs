namespace PetShop_Upgrade.Options
{
    public class AppointmentOptions
    {
        public const string SectionName = "Appointment";
        public string TimeZoneId { get; set; } = "SE Asia Standard Time";
        public TimeSpan OpeningTime { get; set; } = new(8, 0, 0);
        public TimeSpan ClosingTime { get; set; } = new(18, 0, 0);
        public int SlotDurationMinutes { get; set; } = 60;
    }
}
