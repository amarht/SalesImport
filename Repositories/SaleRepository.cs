using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Data;

public class SaleRepository : Repository<Sale>, ISaleRepository {
    private readonly ILogger<SaleRepository> _logger;

    public SaleRepository(AppDbContext context, ILogger<SaleRepository> logger) : base(context) {
        _logger = logger;
    }

    public async Task BulkInsertAsync(List<Sale> sales) {
        if (sales.Count == 0) {
            return;
        }

        var connection =
            (SqlConnection)_context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try {
            var sqlTransaction = (SqlTransaction)transaction.GetDbTransaction();
            using var bulkCopy = new SqlBulkCopy(
                    connection,
                    SqlBulkCopyOptions.TableLock,
                    sqlTransaction);

            bulkCopy.DestinationTableName = "Sales";

            bulkCopy.BatchSize = 100000;

            bulkCopy.BulkCopyTimeout = 0;

            // Column mappings
            bulkCopy.ColumnMappings.Add(
                "SaleNumber",
                "SaleNumber");

            bulkCopy.ColumnMappings.Add(
                "ProductCode",
                "ProductCode");

            bulkCopy.ColumnMappings.Add(
                "Quantity",
                "Quantity");

            bulkCopy.ColumnMappings.Add(
                "SaleDate",
                "SaleDate");

            bulkCopy.ColumnMappings.Add(
                "StoreCode",
                "StoreCode");

            bulkCopy.ColumnMappings.Add(
                "UnitPrice",
                "UnitPrice");

            using var reader = new SaleDataReader(sales);
            await bulkCopy.WriteToServerAsync(reader);
            await transaction.CommitAsync();
        } catch {
            _logger.LogError("Failed to bulk insert {Count} sales, rolling back...", sales.Count);
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<StoreRevenueDto>> GetRevenueByStore() {
        return await _context.Sales
            .AsNoTracking()
            .GroupBy(s => s.StoreCode)
            .Select(g => new StoreRevenueDto {
                    Store = g.Key,
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
                    })
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToListAsync();
    }

    public async Task<List<StoreRevenueDto>> GetNextPageRevenueByStore(int lastSaleNumber, int pageSize) {
        return await _context.Sales
            .AsNoTracking()
            .GroupBy(s => s.StoreCode)
            .Select(g => new StoreRevenueDto {
                    Store = g.Key,
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
                    })
            .OrderByDescending(x => x.Revenue)
            .Skip((lastSaleNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<ProductRevenueDto>> GetRevenueByProduct() {
        return await _context.Sales
            .AsNoTracking()
            .GroupBy(s => s.ProductCode)
            .Select(g => new ProductRevenueDto {
                    Product = g.Key,
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
                    })
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToListAsync();
    }

    public async Task<List<Top5BestProductsDto>> GetTop5BestProducts() {
        return await _context.Sales
            .AsNoTracking()
            .GroupBy(s => s.ProductCode)
            .Select(g => new Top5BestProductsDto {
                    Product = g.Key,
                    Sold = g.Count()
                    })
            .OrderByDescending(x => x.Sold)
            .Take(5)
            .ToListAsync();
    }

    public async Task<List<Sale>> GetSalesByStore(string storeCode) {
        return await _context.Sales
            .AsNoTracking()
            .Where(s => s.StoreCode == storeCode)
            .ToListAsync();
    }

    public async Task<List<Sale>> GetSalesByProduct(string productCode) {
        return await _context.Sales
            .AsNoTracking()
            .Where(s => s.ProductCode == productCode)
            .ToListAsync();
    }

    public async Task<List<Sale>> GetSalesByDate(DateTime date) {
        return await _context.Sales
            .AsNoTracking()
            .Where(s => s.SaleDate.Date == date.Date)
            .ToListAsync();
    }

    public async Task<List<Sale>> GetSalesByMinQuantity(int minQuantity) {
        return await _context.Sales
            .AsNoTracking()
            .Where(s => s.Quantity >= minQuantity)
            .ToListAsync();
    }
}
