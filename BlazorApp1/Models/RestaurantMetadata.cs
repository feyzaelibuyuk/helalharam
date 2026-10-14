namespace BlazorApp1.Models;

public sealed class RestaurantMetadata
{
    public int RestaurantId { get; set; }
    public string CountryCode { get; set; } = "";
    public string City { get; set; } = "";
    public string Branch { get; set; } = "";
    public string Language { get; set; } = "tr";
    public string ConfirmedIngredients { get; set; } = "";
    public bool IsDemo { get; set; }
    public DateTime? LastCheckedUtc { get; set; }
}
