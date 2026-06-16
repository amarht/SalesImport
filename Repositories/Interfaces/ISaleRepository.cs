public interface ISaleRepository : IRepository<Sale> {
    Task BulkInsertAsync(List<Sale> sales);
    Task<List<StoreRevenueDto>> GetRevenueByStore();
    Task<List<StoreRevenueDto>> GetNextPageRevenueByStore(int lastSaleNumber, int pageSize);
    Task<List<ProductRevenueDto>> GetRevenueByProduct();
    Task<List<Top5BestProductsDto>> GetTop5BestProducts();
    Task<List<Sale>> GetSalesByStore(string storeCode);
    Task<List<Sale>> GetSalesByProduct(string productCode);
    Task<List<Sale>> GetSalesByDate(DateTime date);
    Task<List<Sale>> GetSalesByMinQuantity(int minQuantity);
}
