namespace MeetingApp.Core.Models;

public class UserMeeting
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; }
    public bool Status { get; set; }
    public string Description { get; set; } = default!;
    public string? DocumentPath { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}