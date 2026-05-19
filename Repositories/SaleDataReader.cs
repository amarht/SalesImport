using System.Data;

public class SaleDataReader : IDataReader {
    private readonly IEnumerator<Sale> _enumerator;

    public SaleDataReader(IEnumerable<Sale> sales) {
        _enumerator = sales.GetEnumerator();
    }

    public bool Read() {
        return _enumerator.MoveNext();
    }

    public int FieldCount => 6;

    public object GetValue(int i) {
        var sale = _enumerator.Current;
        return i switch {
            0 => sale.SaleNumber,
            1 => sale.ProductCode,
            2 => sale.Quantity,
            3 => sale.SaleDate,
            4 => sale.StoreCode,
            5 => sale.UnitPrice,
            _ => throw new IndexOutOfRangeException()
        };
    }

    public string GetName(int i) {
        return i switch {
            0 => "SaleNumber",
            1 => "ProductCode",
            2 => "Quantity",
            3 => "SaleDate",
            4 => "StoreCode",
            5 => "UnitPrice",
            _ => throw new IndexOutOfRangeException()
        };
    }

    public Type GetFieldType(int i)
    {
        return i switch
        {
            0 => typeof(int),
            1 => typeof(string),
            2 => typeof(int),
            3 => typeof(DateTime),
            4 => typeof(string),
            5 => typeof(float),
            _ => throw new IndexOutOfRangeException()
        };
    }

    public int GetOrdinal(string name) {
        return name switch {
            "SaleNumber" => 0,
            "ProductCode" => 1,
            "Quantity" => 2,
            "SaleDate" => 3,
            "StoreCode" => 4,
            "UnitPrice" => 5,
            _ => throw new IndexOutOfRangeException()
        };
    }

    public void Dispose() {
        _enumerator.Dispose();
    }

    // Minimal required implementations
    public bool IsDBNull(int i) => GetValue(i) == DBNull.Value;

    public object this[int i] => GetValue(i);

    public object this[string name] => GetValue(GetOrdinal(name));

    public void Close() { }

    public DataTable GetSchemaTable() => throw new NotImplementedException();

    public bool NextResult() => false;

    public int Depth => 0;

    public bool IsClosed => false;

    public int RecordsAffected => 0;

    // Unused methods
    public bool GetBoolean(int i) => (bool)GetValue(i);
    public byte GetByte(int i) => (byte)GetValue(i);
    public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) => throw new NotImplementedException();
    public char GetChar(int i) => (char)GetValue(i);
    public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) => throw new NotImplementedException();
    public IDataReader GetData(int i) => throw new NotImplementedException();
    public string GetDataTypeName(int i) => GetFieldType(i).Name;
    public DateTime GetDateTime(int i) => (DateTime)GetValue(i);
    public decimal GetDecimal(int i) => Convert.ToDecimal(GetValue(i));
    public double GetDouble(int i) => Convert.ToDouble(GetValue(i));
    public float GetFloat(int i) => Convert.ToSingle(GetValue(i));
    public Guid GetGuid(int i) => (Guid)GetValue(i);
    public short GetInt16(int i) => Convert.ToInt16(GetValue(i));
    public int GetInt32(int i) => Convert.ToInt32(GetValue(i));
    public long GetInt64(int i) => Convert.ToInt64(GetValue(i));
    public string GetString(int i) => GetValue(i).ToString()!;
    public int GetValues(object[] values) => throw new NotImplementedException();
}
