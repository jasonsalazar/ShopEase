using System.Net;
using System.Net.Mail;
using ShopEase.Application.Interfaces;

namespace ShopEase.Web.Services;

public class SmtpShopEaseEmailSender(IConfiguration configuration) : IShopEaseEmailSender
{
    private readonly IConfiguration _configuration = configuration;

    public async Task SendAsync(
        string email,
        string subject,
        string htmlMessage,
        CancellationToken cancellationToken = default)
    {
        var section = _configuration.GetSection("Smtp");

        var host = section["Host"];
        var port = int.TryParse(section["Port"], out var parsedPort)
            ? parsedPort
            : 587;
        var username = section["Username"];
        var password = section["Password"];
        var from = section["From"];

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(from))
        {
            throw new InvalidOperationException(
                "SMTP settings are missing.");
        }

        using var message = new MailMessage(from, email)
        {
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(username))
        {
            client.Credentials = new NetworkCredential(
                username, password);
        }

        cancellationToken.ThrowIfCancellationRequested();

        await client.SendMailAsync(message, cancellationToken);
    }
}
