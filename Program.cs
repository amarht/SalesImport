using System.Globalization;

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

            // Validate SaleId
            if (!int.TryParse(parts[0], out int saleId))
            {
                yield return (
                    null,
                    $"Line {lineNumber}: invalid SaleId"
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
            if (!decimal.TryParse(
                    parts[4],
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out decimal unitPrice))
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
                    "yyyy-MM-dd",
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
                    SaleId = saleId,
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
    public static void Main() {
        string currentFolder = Directory.GetCurrentDirectory();

        foreach (string file in Directory.EnumerateFiles(currentFolder, "*.csv")) {
            foreach (var result in SaleReader.ReadSales(file)) {
                if (result.Error != null) {
                    Console.WriteLine(result.Error);
                    continue;
                }

                Sale sale = result.Sale!;

                Console.WriteLine(sale.StoreCode);
            
        }
    }
}
