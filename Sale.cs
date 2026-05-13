public sealed class Sale
{
    public int SaleId { get; init; }

    public string StoreCode { get; init; } = "";

    public string ProductCode { get; init; } = "";

    public int Quantity { get; init; }

    public decimal UnitPrice { get; init; }

    public DateTime SaleDate { get; init; }
}
