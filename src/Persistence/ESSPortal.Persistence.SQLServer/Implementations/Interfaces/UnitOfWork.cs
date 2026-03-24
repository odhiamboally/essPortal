
using ESSPortal.Domain.Interfaces;
using ESSPortal.Domain.IRepositories;
using ESSPortal.Persistence.SQLServer.DataContext;
using Microsoft.EntityFrameworkCore;
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

    public void ClearChangeTracker()
    {
        _context.ChangeTracker.Clear();
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var result = await operation();

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public async Task<TResult> ExecuteInTransactionWithRetryAsync<TResult>(Func<Task<TResult>> operation, int maxRetries, int baseDelayMs)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var result = await operation();

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return result;
                }
                catch (DbUpdateConcurrencyException ex) when (attempt < maxRetries)
                {
                    await transaction.RollbackAsync();

                    _context.ChangeTracker.Clear();

                    await Task.Delay(TimeSpan.FromMilliseconds(baseDelayMs * attempt));

                    continue;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            throw new InvalidOperationException("Max retry attempts exceeded due to concurrency conflicts.");
        });
    }

    public async Task ExecuteWithStrategyAsync(Func<Task> operation)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await operation();
        });
    }

    public async Task<TResult> ExecuteWithStrategyAsync<TResult>(Func<Task<TResult>> operation)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            return await operation();
        });
    }

    
}
