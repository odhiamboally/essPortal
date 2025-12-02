using ESSPortal.Application.Configuration;
using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Domain.Interfaces;

namespace ESSPortal.Application.Contracts.Implementations.Services;
internal sealed class UploadService : IUploadService
{
    private readonly IUnitOfWork _unitOfWork;

    public UploadService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

    }

    public Task<ApiResponse<UploadResponse>> CreateAsync(CreateUploadRequest createUploadRequest)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<UploadResponse>> DeleteAsync(int Id)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<List<UploadResponse>>> FindAllAsync(PaginationSetting paginationSetting)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<UploadResponse>> FindByIdAsync(int Id)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<UploadResponse>> UpdateAsync(UpdateUploadRequest request, bool dBWins)
    {
        throw new NotImplementedException();
    }
}
