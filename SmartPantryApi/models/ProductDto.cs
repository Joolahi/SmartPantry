public class ProductDto
{
    public required string  Name { get; set; }
    public required string Barcode { get; set; }
    public required string Brand { get; set; }
    public required double Energy { get; set; }
    public required string BestBefore { get; set; }
    public required string CreatedAt { get; set; } // ISO8601 string
}