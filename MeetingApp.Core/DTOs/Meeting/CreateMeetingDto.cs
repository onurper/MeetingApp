using Microsoft.AspNetCore.Http;

namespace MeetingApp.Core.DTOs.Meeting;

public class CreateMeetingDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public IFormFile Document { get; set; }
}