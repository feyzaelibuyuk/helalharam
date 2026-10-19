namespace BlazorApp1.Models;

public sealed class CorrectionReview
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public int Revision { get; set; }
    public string PreviousStatus { get; set; } = "";
    public string Status { get; set; } = "";
    public string Note { get; set; } = "";
    public string Reviewer { get; set; } = "";
    public DateTime CreatedUtc { get; set; }
}
