using System.Globalization;

public static class CsvStreamReader
{
    public static IEnumerable<Sale> ReadSales(string path)
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

        while ((line = reader.ReadLine()) != null)
        {
            var parts = line.Split(',');

            yield return new Sale
            {
                SaleId = int.Parse(parts[0]),

                StoreCode = parts[1],

                ProductCode = parts[2],

                Quantity = int.Parse(parts[3]),

                UnitPrice = float.Parse(
                    parts[4],
                    CultureInfo.InvariantCulture
                ),

                SaleDate = DateTime.ParseExact(
                    parts[5],
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture
                )
            };
        }
    }
}

class Program {
    public static void Main() {
        var salesCsv = CsvStreamReader.ReadSales("Sales.csv");
        
        foreach (var sale in salesCsv) {
            Console.WriteLine(sale.StoreCode);
        }
    }
}
