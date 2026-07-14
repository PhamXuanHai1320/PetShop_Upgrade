namespace PetShop_Upgrade.DTOS
{
    public class PagedData<T>
    {
        public IEnumerable<T> Items { get; set; } = [];
        public int TotalItems { get; set; }
    }
}
