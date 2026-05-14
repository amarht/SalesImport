using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class SaleReader
{
    public static IEnumerable<(Sale? Sale, string? Error)> ReadSales(string path)
    {
        using var reader = new StreamReader(
            path,
            System.Text.Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 1 << 20
        );

        // Skip header
        reader.ReadLine();

        string? line;
        int lineNumber = 1;

        while ((line = reader.ReadLine()) != null)
        {
            lineNumber++;

            var parts = line.Split(',');

            // Validate column count
            if (parts.Length != 6)
            {
                yield return (
                    null,
                    $"Line {lineNumber}: expected 6 columns, got {parts.Length}"
                );

                continue;
            }

            // Validate SaleNumber
            if (!int.TryParse(parts[0], out int saleNumber))
            {
                yield return (
                    null,
                    $"Line {lineNumber}: invalid SaleNumber"
                );

                continue;
            }

            // Validate Quantity
            if (!int.TryParse(parts[3], out int quantity))
            {
                yield return (
                    null,
                    $"Line {lineNumber}: invalid Quantity"
                );

                continue;
            }

            // Validate UnitPrice
            if (!float.TryParse(
                    parts[4],
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out float unitPrice))
            {
                yield return (
                    null,
                    $"Line {lineNumber}: invalid UnitPrice"
                );

                continue;
            }

            // Validate SaleDate
            if (!DateTime.TryParseExact(
                    parts[5],
                    "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime saleDate))
            {
                yield return (
                    null,
                    $"Line {lineNumber}: invalid SaleDate"
                );

                continue;
            }

            // Additional business validation
            if (quantity <= 0)
            {
                yield return (
                    null,
                    $"Line {lineNumber}: Quantity must be > 0"
                );

                continue;
            }

            if (unitPrice < 0)
            {
                yield return (
                    null,
                    $"Line {lineNumber}: UnitPrice must be >= 0"
                );

                continue;
            }

            yield return (
                new Sale
                {
                    SaleNumber = saleNumber,
                    StoreCode = parts[1],
                    ProductCode = parts[2],
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    SaleDate = saleDate
                },
                null
            );
        }
    }
}

class Program {
    public static async Task Main(string[] args) {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) => {
                    var connectionString = context.Configuration.GetConnectionString("DefaultConnection");

                    // EF Core
                    services.AddDbContext<AppDbContext>(options => {
                            options.UseSqlServer(connectionString);

                            options.UseLoggerFactory(null);

                            options.EnableSensitiveDataLogging(false);
                            options.EnableDetailedErrors(false);
                    });

                    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

                    services.AddScoped<SaleService>();
        })
        .Build();

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.ChangeTracker.AutoDetectChangesEnabled = false;

        var saleService = scope.ServiceProvider.GetRequiredService<SaleService>();

        string currentFolder = Directory.GetCurrentDirectory();

        const int batchSize = 100000;
        List<Sale> batch = new List<Sale>(batchSize);

        foreach (string file in Directory.EnumerateFiles(currentFolder, "*.csv")) {
            foreach (var result in SaleReader.ReadSales(file)) {
                if (result.Error != null) {
                    Console.WriteLine(result.Error);
                    continue;
                }

                batch.Add(result.Sale!);

                if (batch.Count >= batchSize) {
                    await saleService.CreateSalesAsync(batch);
                    await saleService.SaveChangesAsync();
                    db.ChangeTracker.Clear();
                    batch.Clear();
                }
            }
        }

        if (batch.Count > 0) {
            await saleService.CreateSalesAsync(batch);
            await saleService.SaveChangesAsync();
            db.ChangeTracker.Clear();
        }
    }
}
