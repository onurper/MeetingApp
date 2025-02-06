namespace MeetingApp.Web.ViewModels;

public record CreateUserViewModel(string? Name, string? Surname, string? Email, string? Phone, string? Password, IFormFile? ProfileImageFile);