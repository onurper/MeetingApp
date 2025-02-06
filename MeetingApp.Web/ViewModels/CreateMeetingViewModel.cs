namespace MeetingApp.Web.ViewModels;
public record CreateMeetingViewModel(string? Title, string? Description, IFormFile? Document, DateTime StartDate, DateTime EndDate);