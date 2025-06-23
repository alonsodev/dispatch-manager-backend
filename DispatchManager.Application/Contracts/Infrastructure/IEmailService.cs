namespace DispatchManager.Application.Contracts.Infrastructure;

public interface IEmailService
{
    Task SendOrderCreatedNotificationAsync(string customerEmail, string customerName, string orderDetails, CancellationToken cancellationToken = default);
    Task SendOrderStatusUpdatedNotificationAsync(string customerEmail, string customerName, string orderDetails, string newStatus, CancellationToken cancellationToken = default);
}