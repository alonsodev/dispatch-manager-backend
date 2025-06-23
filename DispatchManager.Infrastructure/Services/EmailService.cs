using DispatchManager.Application.Contracts.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace DispatchManager.Infrastructure.Services;

public sealed class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _emailSettings = _configuration.GetSection("EmailSettings").Get<EmailSettings>()
            ?? new EmailSettings();
    }

    public async Task SendOrderCreatedNotificationAsync(
        string customerEmail,
        string customerName,
        string orderDetails,
        CancellationToken cancellationToken = default)
    {
        var subject = "Orden creada satisfactoriamente";
        var body = $@"
            <html>
            <body>
                <h2>Confirmación de Orden</h2>
                <p>Estimado {customerName},</p>
                <p>Tu orden ha sido creada satisfactoriamente!</p>
                <p><strong>Detalle de la Orden:</strong></p>
                <p>{orderDetails}</p>
                <p>Gracias por escoger nuestro servicio de despacho.</p>
                <br>
                <p>Saludos cordiales,<br>Equipo Dispatch Manager</p>
            </body>
            </html>";

        await SendEmailAsync(customerEmail, subject, body, cancellationToken);
    }

    public async Task SendOrderStatusUpdatedNotificationAsync(
        string customerEmail,
        string customerName,
        string orderDetails,
        string newStatus,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Estado de Orden Actualizada - {newStatus}";
        var body = $@"
            <html>
            <body>
                <h2>Actualización del Estado de la Orden</h2>
                <p>Estimado {customerName},</p>
                <p>El estado de su pedido ha sido actualizado.</p>
                <p><strong>Orden:</strong> {orderDetails}</p>
                <p><strong>Nuevo estado:</strong> <span style='color: #007bff; font-weight: bold;'>{newStatus}</span></p>
                <p>Gracias por escoger nuestro servicio de despacho.</p>
                <br>
                <p>Saludos cordiales,<br>Equipo Dispatch Manager</p>
            </body>
            </html>";

        await SendEmailAsync(customerEmail, subject, body, cancellationToken);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
    {
        if (!_emailSettings.EnableEmail)
        {
            _logger.LogInformation("El envío de correos electrónicos está desactivado. ¿Debería enviar a: {Email}, Título: {Subject}", toEmail, subject);
            return;
        }

        try
        {
            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port);
            client.EnableSsl = _emailSettings.EnableSsl;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);

            using var message = new MailMessage();
            message.From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
            message.To.Add(toEmail);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            await client.SendMailAsync(message, cancellationToken);

            _logger.LogInformation("Correo enviado con éxito a: {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo enviar el correo electrónico a: {Email}", toEmail);

            // se podría querer guardar en una cola para retry
            // o enviar a un servicio de notificaciones alternativo
        }
    }
}
