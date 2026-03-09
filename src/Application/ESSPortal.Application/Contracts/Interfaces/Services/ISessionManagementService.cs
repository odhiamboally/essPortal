using ESSPortal.Domain.Entities;
using ESSPortal.Shared.Dtos.Common;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ISessionManagementService
{
    Task<AppResponse<bool>> CheckConcurrentSessionsAsync(string userId);
    Task<AppResponse<bool>> CreateSessionAsync(string userId, string sessionId, string ipAddress, string userAgent);
    Task<AppResponse<string>> CreateSessionAsync(string userId, string sessionId, string ipAddress, string userAgent, string deviceFingerprint);
    Task<AppResponse<bool>> EndSessionAsync(string sessionId);
    Task<AppResponse<bool>> EndAllUserSessionsAsync(string userId, string? excludeSessionId = null);
    Task<AppResponse<List<UserSession>>> GetActiveSessionsAsync(string userId);
    Task<AppResponse<bool>> CleanupExpiredSessionsAsync();
    Task<AppResponse<bool>> IsSessionValidAsync(string sessionId, string userId);
}
