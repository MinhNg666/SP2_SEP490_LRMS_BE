using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Service.Interfaces;

namespace Service.Implementations;
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("EmailSettings");
            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpSettings["Sender"], smtpSettings["SenderName"]),
                Subject = subject,
                Body = message,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            using (var client = new SmtpClient(smtpSettings["SmtpServer"], int.Parse(smtpSettings["Port"])))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]);
                client.EnableSsl = bool.Parse(smtpSettings["EnableSsl"]);

                await client.SendMailAsync(mailMessage);
            }
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error sending email: {ex.Message}");
        }
    }
}
