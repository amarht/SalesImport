public interface ISaleRepository : IRepository<Sale> {
    Task BulkInsertAsync(List<Sale> sales);
}
