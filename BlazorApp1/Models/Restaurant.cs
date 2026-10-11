namespace BlazorApp1.Models;

public sealed class Restaurant
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Dish { get; set; } = "";
    public string PossibleIngredients { get; set; } = "";
    public string Questions { get; set; } = "";
    public string InformationSource { get; set; } = "";
}
