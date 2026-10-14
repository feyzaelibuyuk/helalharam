using System.Text.Json;
using System.Text.RegularExpressions;

namespace HelalRehber.Shared;

public sealed record FavoriteItem(string Kind, string Id, string Name, string Brand)
{
    [System.Text.Json.Serialization.JsonIgnore]
    public string Key => Kind + ":" + Id;
    [System.Text.Json.Serialization.JsonIgnore]
    public string Url => Kind == "catalog" ? "/products/" + Id : "/off?barcode=" + Id;
}
public static class FavoriteList
{
    public const int Limit = 20;
    public static bool Valid(string kind, string id) => kind == "catalog"
        ? int.TryParse(id, out var number) && number > 0 && number.ToString() == id
        : kind == "off" && Regex.IsMatch(id, "\\A(?:[0-9]{8}|[0-9]{12}|[0-9]{13}|[0-9]{14})\\z");
    public static List<FavoriteItem> Read(string? json)
    {
        try
        {
            return (JsonSerializer.Deserialize<List<FavoriteItem>>(json ?? "[]") ?? [])
                .Where(x => x is not null && x.Kind is not null && x.Id is not null && Valid(x.Kind, x.Id))
                .Select(x => Clean(x)).DistinctBy(x => x.Key).Take(Limit).ToList();
        }
        catch (JsonException) { return []; }
    }
    public static FavoriteItem Clean(FavoriteItem item) => item with { Name = Trim(item.Name, 70), Brand = Trim(item.Brand, 40) };
    private static string Trim(string? value, int length) => (value ?? "").Length > length ? value![..length] : value ?? "";
    public static bool Add(List<FavoriteItem> items, FavoriteItem item)
    {
        if (!Valid(item.Kind, item.Id)) return false;
        var index = items.FindIndex(x => x.Key == item.Key);
        if (index >= 0) { items[index] = Clean(item); return true; }
        if (items.Count >= Limit) return false;
        var candidate = new List<FavoriteItem>(items); candidate.Insert(0, Clean(item));
        if (JsonSerializer.Serialize(candidate).Length > 4000) return false;
        items.Insert(0, Clean(item)); return true;
    }
}
