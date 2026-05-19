using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

public class SaleRepository : Repository<Sale>, ISaleRepository {
    public SaleRepository(AppDbContext context) : base(context) {
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

        using var bulkCopy = new SqlBulkCopy(
                connection,
                SqlBulkCopyOptions.TableLock,
                null);

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
    }

}
