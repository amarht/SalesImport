using System.Text;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class SaleReader
{
    public static IEnumerable<(Sale? Sale, string? Error)> ReadSales(string path, int threadNumber, int threadPoolSize)
    {
        if (threadPoolSize <= 0)
            throw new ArgumentException("threadPoolSize must be > 0");

        if (threadNumber < 0 || threadNumber >= threadPoolSize)
            throw new ArgumentException($"Invalid threadNumber {threadNumber}");

        using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        long fileSize = fs.Length;
        long chunkSize = fileSize / threadPoolSize;

        long start = chunkSize * threadNumber;
        long end = (threadNumber == threadPoolSize - 1)
            ? fileSize
            : start + chunkSize;

        fs.Seek(start, SeekOrigin.Begin);

        using StreamReader reader = new StreamReader(
            fs,
            Encoding.UTF8,
            true,
            4096,
            leaveOpen: true);

        // Skip header/cut line
        reader.ReadLine();

        int lineNumber = 0;

        long bytesRead = start;

        string? line = null;

        while ((line = reader.ReadLine()) != null)
        {
            lineNumber++;

            bytesRead += Encoding.UTF8.GetByteCount(line) + Environment.NewLine.Length;

            if (bytesRead > end)
                break;

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
                    services.AddScoped<ISaleRepository, SaleRepository>();

                    services.AddScoped<SaleService>();
        })
        .Build();

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.ChangeTracker.AutoDetectChangesEnabled = false;

        var saleService = scope.ServiceProvider.GetRequiredService<SaleService>();

        if (args.Length > 0 && args[0] == "import") {
            await ImportSalesAsync(scope.ServiceProvider);
        } else {
            await RunQueriesAsync(saleService);
        }
    }

    private static async Task ImportSalesAsync(IServiceProvider services) {
        string currentFolder = Directory.GetCurrentDirectory();

        const int batchSize = 100000;
        const int threadPoolSize = 2;

        List<Task> tasks = new();

        foreach (string file in Directory.EnumerateFiles(currentFolder, "*.csv")) {
            for (int threadNumber = 0; threadNumber < threadPoolSize; threadNumber++) {
                int localThreadNumber = threadNumber;
                tasks.Add(Task.Run(async () => {
                    // Each thread gets own scope + DbContext
                    using var scope = services.CreateScope();

                    var saleService = scope.ServiceProvider.GetRequiredService<SaleService>();

                    List<Sale> batch = new List<Sale>(batchSize);

                    foreach (var result in SaleReader.ReadSales(file, localThreadNumber, threadPoolSize)) {
                        if (result.Error != null) {
                            Console.WriteLine(result.Error);
                            continue;
                        }

                        batch.Add(result.Sale!);

                        if (batch.Count >= batchSize) {
                            await saleService.BulkInsertAsync(batch);
                            batch.Clear();
                        }
                    }

                    if (batch.Count > 0) {
                        await saleService.BulkInsertAsync(batch);
                    }
                }));
            }
        }

        await Task.WhenAll(tasks);

    }

    private static async Task RunQueriesAsync(SaleService saleService) {
        foreach (var strRev in await saleService.GetRevenueByStore()) {
            Console.WriteLine($"{strRev.Store}: {strRev.Revenue} €");
        }

        int pageSize = 10;
        for (int i = 1; i < 5; i++) {
            List<StoreRevenueDto> sR = await saleService.GetNextPageRevenueByStore(i * pageSize, pageSize);
            foreach (var strRev in sR) {
                Console.WriteLine($"{strRev.Store}: {strRev.Revenue} €");
            }
        }

        foreach (var prodRev in await saleService.GetRevenueByProduct()) {
            Console.WriteLine($"{prodRev.Product}: {prodRev.Revenue} €");
        }

        foreach (var prod in await saleService.GetTop5BestProducts()) {
            Console.WriteLine($"{prod.Product}: {prod.Sold}");
        }
    }
}
