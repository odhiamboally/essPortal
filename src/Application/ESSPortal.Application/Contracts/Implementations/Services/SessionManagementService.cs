using ESSPortal.Application.Configuration;
using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Domain.Entities;
using ESSPortal.Domain.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESSPortal.Application.Contracts.Implementations.Services;
internal sealed class SessionManagementService : ISessionManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SessionManagementService> _logger;
    private readonly SessionManagementSettings _sessionSettings;

    public SessionManagementService(
        IUnitOfWork unitOfWork,
        ILogger<SessionManagementService> logger,
        IOptions<SessionManagementSettings> sessionSettings)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _sessionSettings = sessionSettings.Value;
    }

    public async Task<ApiResponse<bool>> CheckConcurrentSessionsAsync(string userId)
    {
        try
        {
            var activeSessions = await _unitOfWork.SessionRepository.GetActiveSessionsByUserIdAsync(userId);
            var sessionCount = activeSessions.Count();

            if (sessionCount >= _sessionSettings.MaxConcurrentSessions)
            {
                _logger.LogWarning("User {UserId} exceeded max concurrent sessions. Active: {Count}, Max: {Max}",
                    userId, sessionCount, _sessionSettings.MaxConcurrentSessions);

                return ApiResponse<bool>.Failure($"Maximum {_sessionSettings.MaxConcurrentSessions} concurrent sessions allowed");
            }

            return ApiResponse<bool>.Success("Concurrent session check passed", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking concurrent sessions for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<ApiResponse<bool>> CreateSessionAsync(string userId, string sessionId, string ipAddress, string userAgent)
    {
        try
        {
            // Check concurrent session limit
            var concurrentCheck = await CheckConcurrentSessionsAsync(userId);
            if (!concurrentCheck.Successful)
            {
                // If at limit, end the oldest session
                var endSessionsResponse = await EndAllUserSessionsAsync(userId);
                
                if (!endSessionsResponse.Successful)
                {
                    _logger.LogError("Failed to end old sessions for user {UserId}: {Message}", userId, endSessionsResponse.Message);
                    return ApiResponse<bool>.Failure("Failed to end old sessions");
                }

                _logger.LogInformation("Ended old sessions for user {UserId} due to concurrent limit", userId);

            }

            var session = new UserSession
            {
                Id = sessionId,
                UserId = userId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = DateTimeOffset.UtcNow,
                LastAccessedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_sessionSettings.SessionTimeoutMinutes),
                IsActive = true
            };

            await _unitOfWork.SessionRepository.CreateAsync(session);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Session created for user {UserId}: {SessionId}", userId, sessionId);
            return ApiResponse<bool>.Success("Session created successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<ApiResponse<bool>> CreateSessionAsync(string userId, string sessionId, string ipAddress, string userAgent, string deviceFingerprint)
    {
        try
        {
            // End all other active sessions for this user
            var activeSessions = await _unitOfWork.SessionRepository.GetActiveSessionsByUserIdAsync(userId);
            var sessionsToEnd = activeSessions
                .Where(s => s.DeviceFingerprint != deviceFingerprint) // Keep session if it's the same device re-authenticating
                .ToList();

            await _unitOfWork.BeginTransactionAsync();

            if (sessionsToEnd.Any())
            {
                _logger.LogInformation("User {UserId} signing in from new device. Ending {Count} concurrent session(s).", userId, sessionsToEnd.Count);
                foreach (var session in sessionsToEnd)
                {
                    session.IsActive = false;
                    session.EndedAt = DateTimeOffset.UtcNow;
                    session.EndReason = "New login from another device.";
                    session.UpdatedAt = DateTimeOffset.UtcNow;
                }
                await _unitOfWork.SessionRepository.UpdateRangeAsync(sessionsToEnd);
            }

            // Check if a session for THIS device already exists and update it
            var existingSessionForDevice = activeSessions.FirstOrDefault(s => s.DeviceFingerprint == deviceFingerprint);

            if (existingSessionForDevice != null)
            {
                _logger.LogInformation("Re-authenticating session for user {UserId} on existing device {DeviceFingerprint}", userId, deviceFingerprint);

                existingSessionForDevice.LastAccessedAt = DateTimeOffset.UtcNow;
                existingSessionForDevice.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_sessionSettings.SessionTimeoutMinutes);
                existingSessionForDevice.IpAddress = ipAddress;
                existingSessionForDevice.UserAgent = userAgent;
                await _unitOfWork.SessionRepository.UpdateAsync(existingSessionForDevice);
            }
            else
            {
                // Create a new session record for the new device
                // Use single timestamp to ensure CreatedAt <= LastAccessedAt

                var now = DateTimeOffset.UtcNow;

                var newSession = new UserSession
                {
                    Id = sessionId,
                    UserId = userId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    DeviceFingerprint = deviceFingerprint, // Store the fingerprint
                    CreatedAt = DateTimeOffset.UtcNow,
                    LastAccessedAt = now,
                    ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_sessionSettings.SessionTimeoutMinutes),
                    IsActive = true
                };

                await _unitOfWork.SessionRepository.CreateAsync(newSession);
            }

            await _unitOfWork.CompleteAsync();

            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Session created/updated for user {UserId} on device {DeviceFingerprint}", userId, deviceFingerprint);
            return ApiResponse<bool>.Success("Session created successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for user: {UserId}", userId);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<ApiResponse<bool>> EndSessionAsync(string sessionId)
    {
        try
        {
            var session = await _unitOfWork.SessionRepository.FindByCondition(x => x.Id == sessionId).FirstOrDefaultAsync();
            if (session == null)
            {
                return ApiResponse<bool>.Success("Session not found", true);
            }

            session.IsActive = false;
            session.EndedAt = DateTimeOffset.UtcNow;
            session.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SessionRepository.UpdateAsync(session);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Session ended: {SessionId} for user: {UserId}", sessionId, session.UserId);
            return ApiResponse<bool>.Success("Session ended successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending session: {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<ApiResponse<bool>> EndAllUserSessionsAsync(string userId, string? excludeSessionId = null)
    {
        try
        {
            var activeSessions = await _unitOfWork.SessionRepository.GetActiveSessionsByUserIdAsync(userId);

            var sessionsToEnd = activeSessions
                .Where(s => s.Id != excludeSessionId)
                .ToList();

            if (sessionsToEnd.Any())
            {
                foreach (var session in sessionsToEnd)
                {
                    session.IsActive = false;
                    session.EndedAt = DateTimeOffset.UtcNow;
                    session.UpdatedAt = DateTimeOffset.UtcNow;
                    session.EndReason = "Concurrent session limit exceeded";
                }

                await _unitOfWork.SessionRepository.UpdateRangeAsync(sessionsToEnd);

            }

            _logger.LogInformation("Ended {Count} sessions for user: {UserId}", sessionsToEnd.Count, userId);
            return ApiResponse<bool>.Success($"Ended {sessionsToEnd.Count} sessions", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending all sessions for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<ApiResponse<List<UserSession>>> GetActiveSessionsAsync(string userId)
    {
        try
        {
            var sessions = await _unitOfWork.SessionRepository.GetActiveSessionsByUserIdAsync(userId);
            var sessionList = sessions.ToList();

            return ApiResponse<List<UserSession>>.Success("Active sessions retrieved", sessionList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active sessions for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<ApiResponse<bool>> CleanupExpiredSessionsAsync()
    {
        try
        {
            var expiredSessions = await _unitOfWork.SessionRepository.GetExpiredSessionsAsync();
            var expiredList = expiredSessions.ToList();

            if (expiredList.Any())
            {
                foreach (var session in expiredList)
                {
                    session.IsActive = false;
                    session.EndedAt = DateTimeOffset.UtcNow;
                    session.EndReason = "Session expired";
                    session.UpdatedAt = DateTimeOffset.UtcNow;
                }

                await _unitOfWork.SessionRepository.UpdateRangeAsync(expiredList);

                _logger.LogInformation("Cleaned up {Count} expired sessions", expiredList.Count);
            }

            return ApiResponse<bool>.Success($"Cleaned up {expiredList.Count} expired sessions", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during session cleanup");
            throw;
        }
    }

    public async Task<ApiResponse<bool>> IsSessionValidAsync(string sessionId, string userId)
    {
        try
        {
            var session = await _unitOfWork.SessionRepository.FindByCondition(s=> s.Id == sessionId).FirstOrDefaultAsync();

            if (session == null || session.UserId != userId)
            {
                return ApiResponse<bool>.Failure("Session not found");
            }

            if (!session.IsActive)
            {
                return ApiResponse<bool>.Failure("Session is not active");
            }

            if (session.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                // Mark as expired
                session.IsActive = false;
                session.EndedAt = DateTimeOffset.UtcNow;
                session.EndReason = "Session expired";
                await _unitOfWork.SessionRepository.UpdateAsync(session);
                await _unitOfWork.CompleteAsync();

                return ApiResponse<bool>.Failure("Session has expired");
            }

            // Update last accessed time
            session.LastAccessedAt = DateTimeOffset.UtcNow;

            // Extend expiry if sliding expiration is enabled
            if (_sessionSettings.SlidingExpiration)
            {
                session.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_sessionSettings.SessionTimeoutMinutes);
            }

            await _unitOfWork.SessionRepository.UpdateAsync(session);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.Success("Session is valid", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating session: {SessionId}", sessionId);
            throw;
        }
    }


}
