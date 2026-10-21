namespace HelalRehber.Shared;
public sealed class FeedbackInput
{
    public Guid SubmissionId { get; set; }
    public string Category { get; set; } = "";
    public string Message { get; set; } = "";
    public string Client { get; set; } = "web";
    public string Version { get; set; } = "";
}
public static class FeedbackRules
{
    public static readonly (string Id, string Label)[] Categories = [("bug", "Hata bildirimi"), ("usability", "Kullanım deneyimi"), ("idea", "Özellik önerisi")];
    public static string Label(string value) => Categories.FirstOrDefault(x => x.Id == value).Label ?? value;
    public static List<string> Validate(FeedbackInput input)
    {
        var errors = new List<string>();
        if (input.SubmissionId == Guid.Empty) errors.Add("Gönderim kimliği eksik. Formu yeniden açın.");
        if (!Categories.Any(x => x.Id == input.Category)) errors.Add("Geri bildirim türünü seçin.");
        if ((input.Message?.Trim().Length ?? 0) is < 10 or > 2000) errors.Add("10–2.000 karakter açıklama yazın.");
        if (input.Client is not ("web" or "android" or "other")) errors.Add("Geçersiz uygulama türü.");
        if ((input.Version?.Length ?? 0) > 40) errors.Add("Sürüm bilgisi çok uzun.");
        return errors;
    }
}
