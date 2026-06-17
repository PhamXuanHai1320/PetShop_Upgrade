namespace PetShop_Upgrade.DTOS
{
    public class ColorDTO
    {
        public int Id { get; set; }
        public string ColorName { get; set; }
        public int IsActive { get; set; } = 1; // 0: InActive; 1: Active
    }
}
