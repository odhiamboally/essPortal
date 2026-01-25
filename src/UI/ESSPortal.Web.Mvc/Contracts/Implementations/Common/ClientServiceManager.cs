using ESSPortal.Shared.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

using Microsoft.Extensions.DependencyInjection;

namespace ESSPortal.Web.Mvc.Contracts.Implementations.Common;

internal sealed class ClientServiceManager : IClientServiceManager
{
    private readonly IServiceProvider _serviceProvider;

    private readonly Lazy<IFileService> _fileService;
    private readonly Lazy<ICacheService> _cacheService;
    private readonly Lazy<IPayloadEncryptionService> _payloadEncryptionService;

    public ClientServiceManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        _fileService = new Lazy<IFileService>(() => _serviceProvider.GetRequiredService<IFileService>());

        _cacheService = new Lazy<ICacheService>(() => _serviceProvider.GetRequiredService<ICacheService>());

        _payloadEncryptionService = new Lazy<IPayloadEncryptionService>(() => _serviceProvider.GetRequiredService<IPayloadEncryptionService>());
    }

    public IFileService FileService => _fileService.Value;
    public ICacheService CacheService => _cacheService.Value;
    public IPayloadEncryptionService PayloadEncryptionService => _payloadEncryptionService.Value;


}
