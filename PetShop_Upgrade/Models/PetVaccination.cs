namespace PetShop_Upgrade.Models
{
    public class PetVaccination
    {
        public int Id { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime UpdateAt { get; set; } = DateTime.Now;
        public int ProductId { get; set; }
        public PetVariant PetVariant { get; set; }
        public int VaccineId { get; set; }
        public Vaccine Vaccine { get; set; }
    }
}
