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

    public async Task SaveChangesAsync() {
        await _repository.SaveChangesAsync();
    }
}
