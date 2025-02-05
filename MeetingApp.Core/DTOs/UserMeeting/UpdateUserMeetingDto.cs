namespace MeetingApp.Core.DTOs.UserMeeting;

public record UpdateUserMeetingDto(string Title, string Description, string DocumentPath, DateTime StartDate, DateTime EndDate);