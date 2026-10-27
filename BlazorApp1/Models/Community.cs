namespace BlazorApp1.Models;
public sealed class CommunityPost
{
    public Guid Id {get;set;}
    public string OwnerKey {get;set;}="";
    public string Payload {get;set;}="";
    public string Status {get;set;}="pending";
    public int Revision {get;set;}
    public DateTime CreatedUtc {get;set;}
}
public sealed class CommunityVote
{
    public Guid PostId {get;set;}
    public string OwnerKey {get;set;}="";
    public string Value {get;set;}="";
}
public sealed class CommunityDecision
{
    public Guid Id {get;set;}
    public Guid PostId {get;set;}
    public int Revision {get;set;}
    public string Status {get;set;}="";
    public string Note {get;set;}="";
    public string Reviewer {get;set;}="";
    public DateTime CreatedUtc {get;set;}
}
