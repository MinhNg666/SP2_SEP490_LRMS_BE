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
    private readonly string _sender;
    private readonly string _senderName;
    private readonly SmtpClient _smtpClient;

    public EmailService(IConfiguration configuration)
    {
        _sender = configuration["EmailSettings:Sender"]
                ?? throw new ArgumentNullException("EmailSettings:Sender configuration is missing");
        _senderName = configuration["EmailSettings:SenderName"]
            ?? throw new ArgumentNullException("EmailSettings:SenderName configuration is missing");

        _smtpClient = new SmtpClient
        {
            Host = configuration["EmailSettings:SmtpServer"],
            Port = int.Parse(configuration["EmailSettings:Port"]),
            EnableSsl = bool.Parse(configuration["EmailSettings:EnableSsl"]),
            Credentials = new NetworkCredential(
                configuration["EmailSettings:Username"],
                configuration["EmailSettings:Password"]
            )
        };
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        string htmlFormattedBody = htmlBody.Replace(Environment.NewLine, "<br>");
    
    var mailMessage = new MailMessage
    {
        From = new MailAddress(_sender, _senderName),
        Subject = subject,
        Body = htmlFormattedBody,
        IsBodyHtml = true // Enable HTML
    };
    mailMessage.To.Add(to);

    await _smtpClient.SendMailAsync(mailMessage);
    }
}
