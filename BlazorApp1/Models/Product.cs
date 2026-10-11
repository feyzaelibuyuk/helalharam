namespace BlazorApp1.Models;

public sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Brand { get; set; } = "";
    public string Barcode { get; set; } = "";
    public string Ingredients { get; set; } = "";
    public string Assessment { get; set; } = "Bilgi bekleniyor";
    public string Note { get; set; } = "";
    public string Source { get; set; } = "";
}
