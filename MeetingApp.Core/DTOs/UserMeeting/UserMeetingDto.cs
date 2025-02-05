namespace MeetingApp.Core.DTOs.UserMeeting;

public record UserMeetingDto(string Title, int UserId, bool Status, string Description, string DocumentPath, DateTime StartDate, DateTime EndDate);