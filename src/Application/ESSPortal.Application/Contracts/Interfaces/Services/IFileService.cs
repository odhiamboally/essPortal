using ESSPortal.Application.Dtos.Common;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IFileService
{
    Task<ApiResponse<string>> SaveLogoAsync(string base64Image);
}
