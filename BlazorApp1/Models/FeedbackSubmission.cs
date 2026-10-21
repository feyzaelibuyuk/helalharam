namespace BlazorApp1.Models;
public sealed class FeedbackSubmission
{
    public Guid Id { get; set; }
    public string Category { get; set; } = "";
    public string Message { get; set; } = "";
    public string Client { get; set; } = "";
    public string Version { get; set; } = "";
    public DateTime CreatedUtc { get; set; }
}
