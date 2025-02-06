namespace MeetingApp.Core.DTOs.UserMeeting;

public record UserMeetingDto(int Id, string Title, int UserId, bool Status, string Description, string DocumentPath, DateTime StartDate, DateTime EndDate);