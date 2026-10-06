using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace CAPNEXAAPI.Utils;

public sealed class EmailSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string FromAddress { get; set; } = string.Empty;
    public string? FromName { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
}

public sealed class EmailSender : IEmailSender
{
    private readonly EmailSettings _settings;

    public EmailSender(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host) ||
            string.IsNullOrWhiteSpace(_settings.FromAddress) ||
            _settings.Port is < 1 or > 65535)
        {
            throw new InvalidOperationException("EmailSettings must include a valid Host, Port, and FromAddress.");
        }

        if (string.IsNullOrWhiteSpace(_settings.UserName) != string.IsNullOrWhiteSpace(_settings.Password))
        {
            throw new InvalidOperationException("EmailSettings.UserName and EmailSettings.Password must both be set for SMTP authentication.");
        }

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrWhiteSpace(_settings.UserName))
        {
            client.Credentials = new NetworkCredential(_settings.UserName, _settings.Password);
        }

        using var mail = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress, _settings.FromName ?? string.Empty),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        mail.To.Add(new MailAddress(email));

        await client.SendMailAsync(mail);
    }
}
