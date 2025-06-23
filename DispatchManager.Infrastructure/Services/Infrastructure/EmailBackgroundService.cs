using DispatchManager.Application.Contracts.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace DispatchManager.Infrastructure.Services.Infrastructure;

/// <summary>
/// Servicio en background para envío asíncrono de emails sin bloquear las operaciones principales
/// </summary>
public sealed class EmailBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailBackgroundService> _logger;
    private readonly ConcurrentQueue<EmailRequest> _emailQueue = new();

    public EmailBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<EmailBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void QueueEmail(EmailRequest request)
    {
        _emailQueue.Enqueue(request);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_emailQueue.TryDequeue(out var emailRequest))
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    switch (emailRequest.Type)
                    {
                        case EmailType.OrderCreated:
                            await emailService.SendOrderCreatedNotificationAsync(
                                emailRequest.ToEmail,
                                emailRequest.CustomerName,
                                emailRequest.Details,
                                stoppingToken);
                            break;

                        case EmailType.OrderStatusUpdated:
                            await emailService.SendOrderStatusUpdatedNotificationAsync(
                                emailRequest.ToEmail,
                                emailRequest.CustomerName,
                                emailRequest.Details,
                                emailRequest.Status ?? "Desconocido",
                                stoppingToken);
                            break;
                    }

                    _logger.LogInformation("Corre enviado satisfactoriamente {Email}", emailRequest.ToEmail);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fallo al enviar correo a {Email}", emailRequest.ToEmail);

                    // Implementar retry logic si es necesario
                    if (emailRequest.RetryCount < 3)
                    {
                        emailRequest.RetryCount++;
                        _emailQueue.Enqueue(emailRequest);
                    }
                }
            }
            else
            {
                // No hay emails en cola, esperar un poco
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}

public sealed class EmailRequest
{
    public EmailType Type { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string? Status { get; set; }
    public int RetryCount { get; set; } = 0;
}

public enum EmailType
{
    OrderCreated,
    OrderStatusUpdated
}