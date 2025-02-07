namespace MeetingApp.Web.Models;

public record MeetingDto(int Id, string Title, int UserId, bool Status, string Description, string DocumentPath, DateTime StartDate, DateTime EndDate);