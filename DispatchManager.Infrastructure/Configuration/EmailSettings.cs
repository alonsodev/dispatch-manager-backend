namespace DispatchManager.Infrastructure.Services;

public sealed class EmailSettings
{
    public bool EnableEmail { get; set; } = false;
    public string SmtpServer { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Dispatch Manager";
}