using ESSPortal.Application.Dtos.Auth;
using ESSPortal.Shared.Dtos.Common;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IEmailService
{
    Task<string> GetTemplateAsync(string templateName);
    Task<string> ApplyTemplateAsync(string templateName, Dictionary<string, string> replacements);
    Task<AppResponse<SendEmailResponse>> SendEmailAsync(SendEmailRequest sendEmailRequest);
}
