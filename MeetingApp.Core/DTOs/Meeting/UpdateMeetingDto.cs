namespace MeetingApp.Core.DTOs.Meeting;

public record UpdateMeetingDto(string Title, string Description, string DocumentPath, DateTime StartDate, DateTime EndDate);