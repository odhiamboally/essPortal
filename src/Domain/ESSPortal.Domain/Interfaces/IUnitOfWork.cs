
using ESSPortal.Domain.IRepositories;

namespace ESSPortal.Domain.Interfaces;
public interface IUnitOfWork : IDisposable
{
    IAppUserRepository UserRepository { get; }
    ITokenRepository TokenRepository { get; }
    IUploadRepository UploadRepository { get; }
    IUserProfileRepository UserProfileRepository { get; }
    ISessionRepository SessionRepository { get; }
    IIpSecurityEventRepository IpSecurityEventRepository { get; }
    IBlockedIpRepository BlockedIpRepository { get; }
    IIpWhitelistRepository IpWhitelistRepository { get; }

    IUserTotpSecretRepository UserTotpSecretRepository { get; }
    IUserBackupCodeRepository UserBackupCodeRepository { get; }
    ITempTotpSecretRepository TempTotpSecretRepository { get;  }




    Task<int> CompleteAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    void ClearChangeTracker();
}
