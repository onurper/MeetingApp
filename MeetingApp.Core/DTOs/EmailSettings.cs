namespace MeetingApp.Core.DTOs;

public class EmailSettings
{
    public string SmtpServer { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = "peronur@gmail.com";
    public string Password { get; set; } = "ahmw tlvt xsxl tlef";
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = "peronur@gmail.com";
    public string FromName { get; set; } = "Onur PER";
}