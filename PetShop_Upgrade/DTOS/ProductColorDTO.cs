namespace PetShop_Upgrade.DTOS
{
    public class ProductColorDTO
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public int? ColorId { get; set; }
        public string ColorName { get; set; }
        public int? Quantity { get; set; }
    }
}
