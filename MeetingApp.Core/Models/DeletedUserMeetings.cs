namespace MeetingApp.Core.Models;

public class DeletedMeetings
{
    public int Id { get; set; }
    public int MeetingId { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; }
    public bool Status { get; set; }
    public string Description { get; set; }
    public string DocumentPath { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime DeletedAt { get; set; }
}