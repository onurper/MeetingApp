using Microsoft.AspNetCore.Http;

namespace MeetingApp.Core.DTOs.CreateDto
{
    public record CreateUserDto(string Name, string Surname, string Email, string Phone, string Password, IFormFile ProfileImageFile);
}
