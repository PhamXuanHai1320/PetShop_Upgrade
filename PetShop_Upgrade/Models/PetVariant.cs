using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Models
{
    public class PetVariant
    {
        public PetVariant() { }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string Gender { get; set; }
        public string Size { get; set; }
        public int Weight { get; set; }
        public IsActive isActive { get; set; } = IsActive.ACTIVE;
        public ICollection<PetHealthRecord> PetHealthRecords { get; set; } = [];
        public ICollection<PetVaccination> PetVaccinations { get; set; } = [];
    }
}
