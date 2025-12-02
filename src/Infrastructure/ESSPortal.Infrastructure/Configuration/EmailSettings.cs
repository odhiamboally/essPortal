namespace ESSPortal.Infrastructure.Configuration;
public class EmailSettings
{
    public string ClientBaseUrl { get; set; } = string.Empty;
    public string SmtpServer { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    
    public bool EnableSsl { get; set; } = true;

    // Optional template path for backend services
    public string? EmailTemplatePath { get; set; }
}
