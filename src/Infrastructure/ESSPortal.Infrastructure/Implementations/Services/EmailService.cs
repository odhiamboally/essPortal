using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Application.Dtos.Auth;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Infrastructure.Configuration;

using FluentEmail.Core;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESSPortal.Infrastructure.Implementations.Services;
internal sealed class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly IFluentEmail _fluentEmail;
    private readonly IMemoryCache _cache;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        IFluentEmail fluentEmail,
        IMemoryCache cache,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value ?? throw new ArgumentNullException(nameof(emailSettings));
        _fluentEmail = fluentEmail ?? throw new ArgumentNullException(nameof(fluentEmail));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GetTemplateAsync(string templateName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(templateName))
                throw new ArgumentException("Template name cannot be null or empty", nameof(templateName));

            var cacheKey = $"EmailTemplate_{templateName}";

            if (_cache.TryGetValue(cacheKey, out string? cachedTemplate) && !string.IsNullOrWhiteSpace(cachedTemplate))
            {
                return cachedTemplate;
            }

            // Simple template path resolution for backend service
            var templatePath = GetTemplatePath(templateName);

            if (!File.Exists(templatePath))
            {
                _logger.LogError("Email template not found: {TemplatePath}", templatePath);
                throw new FileNotFoundException($"Email template '{templateName}' not found");
            }

            var template = await File.ReadAllTextAsync(templatePath);

            _cache.Set(cacheKey, template, TimeSpan.FromHours(1));

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading email template '{TemplateName}'", templateName);
            throw;
        }
    }

    public async Task<string> ApplyTemplateAsync(string templateName, Dictionary<string, string> replacements)
    {
        try
        {
            var template = await GetTemplateAsync(templateName);

            foreach (var replacement in replacements ?? new Dictionary<string, string>())
            {
                template = template.Replace($"@{replacement.Key}", replacement.Value ?? string.Empty);
            }

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying template '{TemplateName}'", templateName);
            throw;
        }
    }

    public async Task<ApiResponse<SendEmailResponse>> SendEmailAsync(SendEmailRequest sendEmailRequest)
    {
        try
        {
            if (sendEmailRequest == null)
                throw new ArgumentNullException(nameof(sendEmailRequest));

            if (string.IsNullOrWhiteSpace(sendEmailRequest.To))
                throw new ArgumentException("Recipient email is required");

            var email = _fluentEmail
                .To(sendEmailRequest.To)
                .Subject(sendEmailRequest.Subject ?? string.Empty)
                .Body(sendEmailRequest.Body ?? string.Empty, true); // true = HTML

            var result = await email.SendAsync();

            if (result.Successful)
            {
                _logger.LogInformation("Email sent successfully to {To}", sendEmailRequest.To);

                return ApiResponse<SendEmailResponse>.Success("Email sent successfully",
                    new SendEmailResponse(
                        Guid.NewGuid().ToString(), 
                        DateTimeOffset.UtcNow, 
                        sendEmailRequest.To, 
                        sendEmailRequest.Subject ?? string.Empty)
                    );
            }
            else
            {
                var errorMessage = string.Join(", ", result.ErrorMessages);
                _logger.LogError("Failed to send email to {To}: {Errors}", sendEmailRequest.To, errorMessage);

                return ApiResponse<SendEmailResponse>.Failure($"Failed to send email: {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}", sendEmailRequest?.To);
            return ApiResponse<SendEmailResponse>.Failure($"Error sending email: {ex.Message}");
        }
    }

    private string GetTemplatePath(string templateName)
    {
        var basePath = _emailSettings.EmailTemplatePath ?? "Templates/Email";

        // If relative path, make it relative to the application directory
        if (!Path.IsPathRooted(basePath))
        {
            basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath);
        }

        return Path.Combine(basePath, $"{templateName}.html");
    }
}
