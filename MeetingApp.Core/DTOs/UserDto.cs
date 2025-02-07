using Microsoft.AspNetCore.Http;

namespace MeetingApp.Core.DTOs;

public record UserDto(string Name, string Surname, string Email, string Phone, string Password, IFormFile ProfileImageFile, string ProfileImagePath);