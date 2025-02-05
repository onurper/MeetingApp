using MeetingApp.Core.DTOs;
using MeetingApp.Core.IServices;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace MeetingApp.Service.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        using (var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port))
        {
            smtp.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
            smtp.EnableSsl = _emailSettings.EnableSsl;

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(to);

            await smtp.SendMailAsync(mailMessage);
        }
    }
}