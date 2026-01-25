using ESSPortal.Shared.Dtos.Common;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IFileService
{
    Task<AppResponse<string>> SaveLogoAsync(string base64Image);
}
