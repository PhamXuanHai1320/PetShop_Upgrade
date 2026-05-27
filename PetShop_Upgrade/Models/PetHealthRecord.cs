namespace PetShop_Upgrade.Models
{
    public class PetHealthRecord
    {
        public PetHealthRecord() { }
        public int Id { get; set; }
        public string Diagnosis { get; set; }
        public string ClinicName { get; set; }
        public string Note { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public int ProductId { get; set; }
        public PetVariant PetVariant { get; set; }
    }
}
