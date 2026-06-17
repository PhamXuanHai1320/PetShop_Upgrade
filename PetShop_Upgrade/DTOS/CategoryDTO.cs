namespace PetShop_Upgrade.DTOS
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProductType { get; set; }
        public int IsActive { get; set; } = 1;
    }
}
