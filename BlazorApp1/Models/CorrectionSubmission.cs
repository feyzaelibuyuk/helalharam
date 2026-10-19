namespace BlazorApp1.Models;

public sealed class CorrectionSubmission
{
    public Guid Id { get; set; }
    public string Kind { get; set; } = "";
    public string TargetId { get; set; } = "";
    public string TargetName { get; set; } = "";
    public string Field { get; set; } = "";
    public string Suggestion { get; set; } = "";
    public string SourceUrl { get; set; } = "";
    public string Status { get; set; } = "pending";
    public DateTime CreatedUtc { get; set; }
}
