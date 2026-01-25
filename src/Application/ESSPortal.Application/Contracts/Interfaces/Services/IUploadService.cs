using ESSPortal.Application.Configuration;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Leave;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IUploadService
{
    Task<AppResponse<UploadResponse>> CreateAsync(CreateUploadRequest createUploadRequest);
    Task<AppResponse<List<UploadResponse>>> FindAllAsync(PaginationSetting paginationSetting);
    Task<AppResponse<UploadResponse>> FindByIdAsync(int Id);
    Task<AppResponse<UploadResponse>> UpdateAsync(UpdateUploadRequest request, bool dBWins);
    Task<AppResponse<UploadResponse>> DeleteAsync(int Id);
}
