using ESSPortal.Application.Configuration;
using ESSPortal.Application.Contracts.Interfaces.Services;

using ESSPortal.Domain.Interfaces;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Leave;

namespace ESSPortal.Application.Contracts.Implementations.Services;
internal sealed class UploadService : IUploadService
{
    private readonly IUnitOfWork _unitOfWork;

    public UploadService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

    }

    public Task<AppResponse<UploadResponse>> CreateAsync(CreateUploadRequest createUploadRequest)
    {
        throw new NotImplementedException();
    }

    public Task<AppResponse<UploadResponse>> DeleteAsync(int Id)
    {
        throw new NotImplementedException();
    }

    public Task<AppResponse<List<UploadResponse>>> FindAllAsync(PaginationSetting paginationSetting)
    {
        throw new NotImplementedException();
    }

    public Task<AppResponse<UploadResponse>> FindByIdAsync(int Id)
    {
        throw new NotImplementedException();
    }

    public Task<AppResponse<UploadResponse>> UpdateAsync(UpdateUploadRequest request, bool dBWins)
    {
        throw new NotImplementedException();
    }
}
