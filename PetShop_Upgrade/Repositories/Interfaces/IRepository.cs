namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetById(int id);
        Task<IEnumerable<T>> GetAll();
        Task<IEnumerable<T>> GetAll(int page, int pageSize);
        Task Add(T entity);
        Task Update(T entity);
        Task Delete(T entity);
    }
}
