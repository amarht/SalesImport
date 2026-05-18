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

        bulkCopy.BatchSize = 5000;

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

        var table = CreateDataTable(sales);

        await bulkCopy.WriteToServerAsync(table);
    }

    private DataTable CreateDataTable(List<Sale> sales) {
        var table = new DataTable();

        // NO identity column
        table.Columns.Add("SaleNumber", typeof(string));
        table.Columns.Add("ProductCode", typeof(string));
        table.Columns.Add("Quantity", typeof(int));
        table.Columns.Add("SaleDate", typeof(DateTime));
        table.Columns.Add("StoreCode", typeof(string));
        table.Columns.Add("UnitPrice", typeof(float));

        foreach (var sale in sales)
        {
            table.Rows.Add(
                sale.SaleNumber,
                sale.ProductCode,
                sale.Quantity,
                sale.SaleDate,
                sale.StoreCode,
                sale.UnitPrice
            );
        }

        return table;
    }
}
