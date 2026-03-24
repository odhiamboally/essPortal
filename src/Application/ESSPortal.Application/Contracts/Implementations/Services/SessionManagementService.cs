using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Domain.Entities;
using ESSPortal.Domain.Interfaces;
using ESSPortal.Domain.IRepositories;
using ESSPortal.Shared.Configuration;
using ESSPortal.Shared.Dtos.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polly;

namespace ESSPortal.Application.Contracts.Implementations.Services;
internal sealed class SessionManagementService(
    IUnitOfWork unitOfWork,
    ILogger<SessionManagementService> logger,
    IOptions<SessionManagementSettings> sessionSettings) : ISessionManagementService
{
    private readonly SessionManagementSettings _sessionSettings = sessionSettings.Value;

    public async Task<AppResponse<bool>> CheckConcurrentSessionsAsync(string userId)
    {
        try
        {
            var activeSessions = await unitOfWork.SessionRepository.GetActiveSessionsByUserIdAsync(userId);
            var sessionCount = activeSessions.Count();

            if (sessionCount >= _sessionSettings.MaxConcurrentSessions)
            {
                logger.LogWarning("User {UserId} exceeded max concurrent sessions. Active: {Count}, Max: {Max}",
                    userId, sessionCount, _sessionSettings.MaxConcurrentSessions);

                return AppResponse<bool>.Failure($"Maximum {_sessionSettings.MaxConcurrentSessions} concurrent sessions allowed");
            }

            return AppResponse<bool>.Success("Concurrent session check passed", true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking concurrent sessions for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<AppResponse<string>> CreateOrUpdateSessionAsync(string userId, string sessionId, string ipAddress, string userAgent, string deviceFingerprint)
    {
        try
        {
            return await unitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
            {
                var now = DateTimeOffset.UtcNow;

                var activeSessions = await unitOfWork
                    .SessionRepository
                    .GetActiveSessionsByUserIdAsync(userId);

                var activeSessionsList = activeSessions.ToList();

                // Group A
                var currentSession = activeSessionsList
                    .FirstOrDefault(s => s.DeviceFingerprint == deviceFingerprint);

                // Group B
                var duplicatesOnThisDevice = activeSessionsList
                    .Where(s => s.DeviceFingerprint == deviceFingerprint &&
                               (currentSession == null || s.Id != currentSession.Id))
                    .ToList();

                // Group C
                var sessionsOnOtherDevices = activeSessionsList
                    .Where(s => s.DeviceFingerprint != deviceFingerprint)
                    .ToList();

                // Deactivate unwanted sessions
                var toDeactivate = duplicatesOnThisDevice
                    .Concat(sessionsOnOtherDevices)
                    .ToList();

                if (toDeactivate.Any())
                {
                    foreach (var s in toDeactivate)
                    {
                        s.IsActive = false;
                        s.EndedAt = now;
                        s.EndReason = s.DeviceFingerprint == deviceFingerprint
                            ? "Duplicate session cleanup"
                            : "New login from another device.";
                        s.UpdatedAt = now;
                    }

                    await unitOfWork.SessionRepository.UpdateRangeAsync(toDeactivate);
                }

                string finalSessionId;

                if (currentSession != null)
                {
                    currentSession.IsActive = true;
                    currentSession.LastAccessedAt = now;
                    currentSession.ExpiresAt = now.AddMinutes(_sessionSettings.SessionTimeoutMinutes);
                    currentSession.IpAddress = ipAddress;
                    currentSession.UserAgent = userAgent;
                    currentSession.UpdatedAt = now;

                    await unitOfWork.SessionRepository.UpdateAsync(currentSession);

                    finalSessionId = currentSession.Id;
                }
                else
                {
                    var newSession = new UserSession
                    {
                        Id = sessionId,
                        UserId = userId,
                        IpAddress = ipAddress,
                        UserAgent = userAgent,
                        DeviceFingerprint = deviceFingerprint,
                        CreatedAt = now,
                        LastAccessedAt = now,
                        ExpiresAt = now.AddMinutes(_sessionSettings.SessionTimeoutMinutes),
                        IsActive = true
                    };

                    await unitOfWork.SessionRepository.CreateAsync(newSession);

                    finalSessionId = newSession.Id;
                }

                logger.LogInformation(
                    "Session created/updated for user {UserId} on device {DeviceFingerprint}",
                    userId,
                    deviceFingerprint);

                return AppResponse<string>.Success("Session managed successfully", finalSessionId);
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating session for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<AppResponse<bool>> EndSessionAsync(string sessionId)
    {
        try
        {
            var session = await unitOfWork.SessionRepository.FindByCondition(x => x.Id == sessionId).FirstOrDefaultAsync();
            if (session == null)
            {
                return AppResponse<bool>.Success("Session not found", true);
            }

            session.IsActive = false;
            session.EndedAt = DateTimeOffset.UtcNow;
            session.UpdatedAt = DateTimeOffset.UtcNow;

            await unitOfWork.SessionRepository.UpdateAsync(session);
            await unitOfWork.CompleteAsync();

            logger.LogInformation("Session ended: {SessionId} for user: {UserId}", sessionId, session.UserId);
            return AppResponse<bool>.Success("Session ended successfully", true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ending session: {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<AppResponse<bool>> EndAllUserSessionsAsync(string userId, string? excludeSessionId = null)
    {
        try
        {
            var activeSessions = await unitOfWork.SessionRepository.GetActiveSessionsByUserIdAsync(userId);

            var sessionsToEnd = activeSessions.Where(s => s.Id != excludeSessionId).ToList();
                
            if (sessionsToEnd.Any())
            {
                foreach (var session in sessionsToEnd)
                {
                    session.IsActive = false;
                    session.EndedAt = DateTimeOffset.UtcNow;
                    session.UpdatedAt = DateTimeOffset.UtcNow;
                    session.EndReason = "Concurrent session limit exceeded";
                }

                await unitOfWork.SessionRepository.UpdateRangeAsync(sessionsToEnd);

                // IMPORTANT: Save changes!
                await unitOfWork.CompleteAsync();
            }

            logger.LogInformation("Ended {Count} sessions for user: {UserId}", sessionsToEnd.Count, userId);
            return AppResponse<bool>.Success($"Ended {sessionsToEnd.Count} sessions", true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ending all sessions for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<AppResponse<List<UserSession>>> GetActiveSessionsAsync(string userId)
    {
        try
        {
            var sessions = await unitOfWork.SessionRepository.GetActiveSessionsByUserIdAsync(userId);
            var sessionList = sessions.ToList();

            return AppResponse<List<UserSession>>.Success("Active sessions retrieved", sessionList);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting active sessions for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<AppResponse<bool>> CleanupExpiredSessionsAsync()
    {
        try
        {
            var expiredSessions = await unitOfWork.SessionRepository.GetExpiredSessionsAsync();
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

                await unitOfWork.SessionRepository.UpdateRangeAsync(expiredList);

                logger.LogInformation("Cleaned up {Count} expired sessions", expiredList.Count);
            }

            return AppResponse<bool>.Success($"Cleaned up {expiredList.Count} expired sessions", true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during session cleanup");
            throw;
        }
    }

    public async Task<AppResponse<bool>> IsSessionValidAsync(string sessionId, string userId)
    {
        var session = await unitOfWork.SessionRepository
                .FindByCondition(s => s.Id == sessionId && s.UserId == userId).FirstOrDefaultAsync() ;

        if (session is null)
            return AppResponse<bool>.Failure("Session not found");

        if (!session.IsActive)
            return AppResponse<bool>.Failure("Session is not active");

        session.LastAccessedAt = DateTimeOffset.UtcNow;
        session.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        await unitOfWork.CompleteAsync();

        return AppResponse<bool>.Success("Session is valid", true);
    }

    public async Task<AppResponse<bool>> IsSessionValidAsync_(string sessionId, string userId)
    {
        try
        {
            var session = await unitOfWork.SessionRepository
                .FindByCondition(s => s.Id == sessionId && s.UserId == userId)
                .AsNoTracking() 
                .FirstOrDefaultAsync();

            if (session == null || session.UserId != userId)
            {
                // If not tracked, then and only then, go to the database
                if (session == null)
                {
                    session = await unitOfWork.SessionRepository
                        .FindByCondition(s => s.Id == sessionId && s.UserId == userId)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
                }
            }

            if (session == null || session.UserId != userId) 
                return AppResponse<bool>.Failure("Session not found");

            if (!session.IsActive)
            {
                return AppResponse<bool>.Failure("Session is not active");
            }

            if (session.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                // Mark as expired
                session.IsActive = false;
                session.EndedAt = DateTimeOffset.UtcNow;
                session.EndReason = "Session expired";


                await unitOfWork.SessionRepository.UpdateAsync(session);
                await unitOfWork.CompleteAsync();

                return AppResponse<bool>.Failure("Session has expired");
            }

            // Update last accessed time
            session.LastAccessedAt = DateTimeOffset.UtcNow;

            // Extend expiry if sliding expiration is enabled
            if (_sessionSettings.SlidingExpiration)
            {
                session.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_sessionSettings.SessionTimeoutMinutes);
            }

            await unitOfWork.SessionRepository.UpdateAsync(session);
            await unitOfWork.CompleteAsync();

            return AppResponse<bool>.Success("Session is valid", true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating session: {SessionId}", sessionId);
            throw;
        }
    }


}
