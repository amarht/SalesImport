public sealed class Sale
{
    public int SaleId { get; init; }

    public int SaleNumber { get; init; }

    public string StoreCode { get; init; } = "";

    public string ProductCode { get; init; } = "";

    public int Quantity { get; init; }

    public float UnitPrice { get; init; }

    public DateTime SaleDate { get; init; }
}
