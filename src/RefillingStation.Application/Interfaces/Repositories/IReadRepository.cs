namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface IReadRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
    }
}
