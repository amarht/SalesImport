public class SaleService {
    private readonly IRepository<Sale> _repository;

    public SaleService(IRepository<Sale> repository) {
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

    public async Task SaveChangesAsync() {
        await _repository.SaveChangesAsync();
    }
}
