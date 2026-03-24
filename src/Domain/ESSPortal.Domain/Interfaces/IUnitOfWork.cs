
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


    /// <summary>
    /// Executes the specified asynchronous operation within a transaction and returns the result.
    /// </summary>
    /// <remarks>If the operation completes successfully, the transaction is committed. If the operation
    /// throws an exception, the transaction is rolled back. The operation should not commit or roll back the
    /// transaction directly.</remarks>
    /// <typeparam name="TResult">The type of the result returned by the asynchronous operation.</typeparam>
    /// <param name="operation">A function that represents the asynchronous operation to execute within the transaction. The function must
    /// return a task that produces a result of type TResult.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the value returned by the operation.</returns>
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation);

    /// <summary>
    /// Executes the specified asynchronous operation within a transaction, automatically retrying the operation if a
    /// transient failure occurs.
    /// </summary>
    /// <remarks>If the operation fails due to a transient error, it is retried up to the specified number of
    /// times with an increasing delay between attempts. If all retries fail, the last encountered exception is
    /// propagated to the caller.</remarks>
    /// <typeparam name="TResult">The type of the result returned by the operation.</typeparam>
    /// <param name="operation">A function that represents the asynchronous operation to execute within the transaction. The function should
    /// return a task that produces the result of type TResult.</param>
    /// <param name="maxRetries">The maximum number of times to retry the operation if a transient failure occurs. Must be zero or greater. The
    /// default is 3.</param>
    /// <param name="baseDelayMs">The base delay, in milliseconds, to wait between retry attempts. The delay may be increased exponentially with
    /// each retry. Must be zero or greater. The default is 50.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the value returned by the operation
    /// if it succeeds.</returns>
    Task<TResult> ExecuteInTransactionWithRetryAsync<TResult>(Func<Task<TResult>> operation, int maxRetries = 3, int baseDelayMs = 50);

    Task<int> CompleteAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    void ClearChangeTracker();
}
