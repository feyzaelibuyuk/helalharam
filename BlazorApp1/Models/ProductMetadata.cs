namespace BlazorApp1.Models;

// Ülke, dil ve kanıt bilgisi üründen ayrı tutulur; eski ürün kayıtları korunur.
public sealed class ProductMetadata
{
    public int ProductId { get; set; }
    public string Category { get; set; } = "";
    public string CountryCode { get; set; } = "";
    public string IngredientsLanguage { get; set; } = "";
    public string OriginalIngredients { get; set; } = "";
    public string SourceUrl { get; set; } = "";
    public DateTime? LastCheckedUtc { get; set; }
    public bool IsVerified { get; set; }
    public bool IsDemo { get; set; }
}

public sealed class ProductRecord
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Brand { get; init; } = "";
    public string Barcode { get; init; } = "";
    public string Ingredients { get; init; } = "";
    public string Assessment { get; init; } = "";
    public string Note { get; init; } = "";
    public string Source { get; init; } = "";
    public string Category { get; init; } = "";
    public string CountryCode { get; init; } = "";
    public string IngredientsLanguage { get; init; } = "";
    public string OriginalIngredients { get; init; } = "";
    public string SourceUrl { get; init; } = "";
    public DateTime? LastCheckedUtc { get; init; }
    public bool IsVerified { get; init; }
    public bool IsDemo { get; init; }
}
public sealed record ProductAlternative(ProductRecord Product, string Reason);
public sealed record ProductDetail(ProductRecord Product, List<ProductAlternative> Alternatives, string AlternativeMessage);
