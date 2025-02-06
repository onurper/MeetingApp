namespace MeetingApp.Core.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Password { get; set; }
    public string ProfileImagePath { get; set; }
    public DateTime CreatedDateTime { get; set; }
}