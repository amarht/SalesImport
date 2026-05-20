public class SaleService {
    private readonly ISaleRepository _repository;

    public SaleService(ISaleRepository repository) {
        _repository = repository;
    }

    public async Task CreateSaleAsync(Sale sale) {
        if (sale.Quantity <= 0) {
            throw new Exception("Quantity must be greater than 0");
        }

        if (sale.UnitPrice < 0) {
            throw new Exception("Unit Price must be positive");
        }

        await _repository.AddAsync(sale);
    }

    public async Task CreateSalesAsync(List<Sale> sales) {
        await _repository.AddRangeAsync(sales);
    }

    public async Task BulkInsertAsync(List<Sale> sales) {
        await _repository.BulkInsertAsync(sales);
    }

    public async Task<List<StoreRevenueDto>> GetRevenueByStore() {
        return await _repository.GetRevenueByStore();
    }

    public async Task<List<ProductRevenueDto>> GetRevenueByProduct() {
        return await _repository.GetRevenueByProduct();
    }

    public async Task<List<Top5BestProductsDto>> GetTop5BestProducts() {
        return await _repository.GetTop5BestProducts();
    }

    public async Task<List<Sale>> GetSalesByStore(string storeCode) {
        return await _repository.GetSalesByStore(storeCode);
    }

    public async Task<List<Sale>> GetSalesByProduct(string productCode) {
        return await _repository.GetSalesByProduct(productCode);
    }

    public async Task<List<Sale>> GetSalesByDate(DateTime date) {
        return await _repository.GetSalesByDate(date);
    }

    public async Task<List<Sale>> GetSalesByMinQuantity(int minQuantity) {
        return await _repository.GetSalesByMinQuantity(minQuantity);
    }

    public async Task SaveChangesAsync() {
        await _repository.SaveChangesAsync();
    }
}
