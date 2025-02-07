namespace MeetingApp.Web.Models;

public record UserDto(string Name, string Surname, string Email, string Phone, string Password, IFormFile ProfileImageFile);