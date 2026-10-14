using System.Text.RegularExpressions;
namespace helalharam;
public static class ProductBarcode
{
    public static bool IsValid(string? value) => value is not null && Regex.IsMatch(value.Trim(), "\\A(?:[0-9]{8}|[0-9]{12}|[0-9]{13}|[0-9]{14})\\z");
}
