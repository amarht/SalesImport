public interface IRepository<T> where T : class {
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);

    Task AddAsync(T entity);
    Task AddRangeAsync(List<T> entities);
    void Update(T entity);
    void Delete(T entity);
    Task BulkInsertAsync(List<T> entities);

    Task SaveChangesAsync();
}
