using ESSPortal.Application.Dtos.Auth;
using ESSPortal.Application.Dtos.Common;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IEmailService
{
    Task<string> GetTemplateAsync(string templateName);
    Task<string> ApplyTemplateAsync(string templateName, Dictionary<string, string> replacements);
    Task<ApiResponse<SendEmailResponse>> SendEmailAsync(SendEmailRequest sendEmailRequest);
}
