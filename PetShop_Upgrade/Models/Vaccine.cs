namespace PetShop_Upgrade.Models
{
    public class Vaccine
    {
        public Vaccine() { }
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<PetVaccination> PetVaccinations { get; set; } = [];
    }
}
