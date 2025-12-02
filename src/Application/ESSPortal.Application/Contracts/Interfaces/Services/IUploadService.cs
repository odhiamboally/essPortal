using ESSPortal.Application.Configuration;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Leave;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IUploadService
{
    Task<ApiResponse<UploadResponse>> CreateAsync(CreateUploadRequest createUploadRequest);
    Task<ApiResponse<List<UploadResponse>>> FindAllAsync(PaginationSetting paginationSetting);
    Task<ApiResponse<UploadResponse>> FindByIdAsync(int Id);
    Task<ApiResponse<UploadResponse>> UpdateAsync(UpdateUploadRequest request, bool dBWins);
    Task<ApiResponse<UploadResponse>> DeleteAsync(int Id);
}
