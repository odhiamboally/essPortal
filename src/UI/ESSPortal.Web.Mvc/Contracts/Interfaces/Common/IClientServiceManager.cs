

using ESSPortal.Shared.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Common;

public interface IClientServiceManager
{

    IFileService FileService { get; }
    ICacheService CacheService { get; }
    IPayloadEncryptionService PayloadEncryptionService { get; }
    
    
    



}
