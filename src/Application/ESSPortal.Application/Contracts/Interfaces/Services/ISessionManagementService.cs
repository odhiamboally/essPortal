using ESSPortal.Application.Dtos.Common;
using ESSPortal.Domain.Entities;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ISessionManagementService
{
    Task<ApiResponse<bool>> CheckConcurrentSessionsAsync(string userId);
    Task<ApiResponse<bool>> CreateSessionAsync(string userId, string sessionId, string ipAddress, string userAgent);
    Task<ApiResponse<bool>> CreateSessionAsync(string userId, string sessionId, string ipAddress, string userAgent, string deviceFingerprint);
    Task<ApiResponse<bool>> EndSessionAsync(string sessionId);
    Task<ApiResponse<bool>> EndAllUserSessionsAsync(string userId, string? excludeSessionId = null);
    Task<ApiResponse<List<UserSession>>> GetActiveSessionsAsync(string userId);
    Task<ApiResponse<bool>> CleanupExpiredSessionsAsync();
    Task<ApiResponse<bool>> IsSessionValidAsync(string sessionId, string userId);
}
