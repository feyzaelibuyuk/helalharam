namespace BlazorApp1.Models;
public sealed class CertificateEvidence
{
    public Guid Id { get; set; }
    public string Payload { get; set; }="";
    public string Fingerprint { get; set; }="";
    public string FileKey { get; set; }="";
    public string FileName { get; set; }="";
    public string Mime { get; set; }="";
    public long FileBytes { get; set; }
    public string Sha256 { get; set; }="";
    public string Status { get; set; }="pending";
    public int Revision { get; set; }
    public DateTime CreatedUtc { get; set; }
}
public sealed class CertificateEvidenceReview
{
    public Guid Id { get; set; } public Guid SubmissionId { get; set; } public int Revision { get; set; }
    public string Status { get; set; }="";public string Note { get; set; }="";public string ChecksJson { get; set; }="";
    public string Reviewer { get; set; }="";public DateTime CreatedUtc { get; set; }
}
