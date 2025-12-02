
using ESSPortal.Domain.Interfaces;
using ESSPortal.Domain.IRepositories;
using ESSPortal.Persistence.SQLServer.DataContext;
using Microsoft.EntityFrameworkCore.Storage;

namespace ESSPortal.Persistence.SQLServer.Implementations.Intefaces;
public class UnitOfWork : IUnitOfWork
{

    public IAppUserRepository UserRepository { get; private set; }
    public ITokenRepository TokenRepository { get; private set; }
    public IUploadRepository UploadRepository { get; private set; }
    public IUserProfileRepository UserProfileRepository { get; private set; }
    public ISessionRepository SessionRepository { get; private set; }
    public IIpSecurityEventRepository IpSecurityEventRepository { get; private set; }
    public IBlockedIpRepository BlockedIpRepository { get; private set; }
    public IIpWhitelistRepository IpWhitelistRepository { get; private set; }

    public IUserTotpSecretRepository UserTotpSecretRepository { get; private set; }
    public IUserBackupCodeRepository UserBackupCodeRepository { get; private set; }
    public ITempTotpSecretRepository TempTotpSecretRepository { get; private set; }



    private IDbContextTransaction? _transaction;
    private readonly DBContext _context;

    public UnitOfWork(
        IAppUserRepository userRepository,
        ITokenRepository tokenRepository,
        IUploadRepository uploadRepository,
        IUserProfileRepository userProfileRepository,
        ISessionRepository sessionRepository,
        IIpSecurityEventRepository ipSecurityRepository,
        IBlockedIpRepository blockedIpRepository,
        IIpWhitelistRepository ipWhitelistRepository,
        IUserBackupCodeRepository userBackupCodeRepository,
        IUserTotpSecretRepository userTotpSecretRepository,
        ITempTotpSecretRepository tempTotpSecretRepository,


        DBContext Context


        )
    {
        UserRepository = userRepository;
        TokenRepository = tokenRepository;
        UploadRepository = uploadRepository;
        UserProfileRepository = userProfileRepository;
        SessionRepository = sessionRepository;
        IpSecurityEventRepository = ipSecurityRepository;
        BlockedIpRepository = blockedIpRepository;
        IpWhitelistRepository = ipWhitelistRepository;
        UserBackupCodeRepository = userBackupCodeRepository;
        TempTotpSecretRepository = tempTotpSecretRepository;
        UserTotpSecretRepository = userTotpSecretRepository;

        _context = Context;
        


    }

    public async Task<int> CompleteAsync()
    {
        var result = await _context.SaveChangesAsync();
        return result!;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);

    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _context.Dispose();
        }
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
