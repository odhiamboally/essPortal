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

    public async Task<AppResponse<bool>> CheckConcurrentSessionsAsync(string userId)
    {
        try
        {
            var activeSessions = await _unitOfWork.SessionRepository.GetActiveSessionsByUserIdAsync(userId);
            var sessionCount = activeSessions.Count();

            if (sessionCount >= _sessionSettings.MaxConcurrentSessions)
            {
                _logger.LogWarning("User {UserId} exceeded max concurrent sessions. Active: {Count}, Max: {Max}",
                    userId, sessionCount, _sessionSettings.MaxConcurrentSessions);

                return AppResponse<bool>.Failure($"Maximum {_sessionSettings.MaxConcurrentSessions} concurrent sessions allowed");
            }

            return AppResponse<bool>.Success("Concurrent session check passed", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking concurrent sessions for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<AppResponse<bool>> CreateSessionAsync(string userId, string sessionId, string ipAddress, string userAgent)
    {
        try
        {
            // Check concurrent session limit
            var concurrentCheck = await CheckConcurrentSessionsAsync(userId);
            if (!concurrentCheck.Successful)
            {
                // If at limit, end the oldest sessions
                var endSessionsResponse = await EndAllUserSessionsAsync(userId);

                if (!endSessionsResponse.Successful)
                {
                    _logger.LogError("Failed to end old sessions for user {UserId}: {Message}",
                        userId, endSessionsResponse.Message);
                    return AppResponse<bool>.Failure("Failed to end old sessions");
                }

            }

            var now = DateTimeOffset.UtcNow;
            var session = new UserSession
            {
                Id = sessionId,
                UserId = userId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = now,
                LastAccessedAt = now,
                ExpiresAt = now.AddMinutes(_sessionSettings.SessionTimeoutMinutes),
                IsActive = true
            };

            await _unitOfWork.SessionRepository.CreateAsync(session);
            await _unitOfWork.CompleteAsync();

            return AppResponse<bool>.Success("Session created successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<AppResponse<bool>> CreateSessionAsync_(string userId, string sessionId, string ipAddress, string userAgent, string deviceFingerprint)
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
            return AppResponse<bool>.Success("Session created successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for user: {UserId}", userId);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<AppResponse<string>> CreateSessionAsync(string userId, string sessionId, string ipAddress, string userAgent, string deviceFingerprint)
    {
        const int maxRetries = 3;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var now = DateTimeOffset.UtcNow;

                // 1. Get ALL potentially active sessions (ignore the strict expiry here to catch 'zombie' sessions)
                var activeSessions = await _unitOfWork.SessionRepository.GetActiveSessionsByUserIdAsync(userId);
                var activeSessionsList = activeSessions.ToList();

                // 2. Separate sessions into three groups:
                // Group A: The specific session we want to use/revive for THIS device
                var currentSession = activeSessionsList.FirstOrDefault(s => s.DeviceFingerprint == deviceFingerprint);

                // Group B: Duplicate sessions for THIS device that we should kill
                var duplicatesOnThisDevice = activeSessionsList
                    .Where(s => s.DeviceFingerprint == deviceFingerprint && (currentSession == null || s.Id != currentSession.Id))
                    .ToList();

                // Group C: Sessions on OTHER devices that we must kill (Concurrency policy)
                var sessionsOnOtherDevices = activeSessionsList
                    .Where(s => s.DeviceFingerprint != deviceFingerprint)
                    .ToList();

                // 3. Process Group B & C (The ones to deactivate)
                var toDeactivate = duplicatesOnThisDevice.Concat(sessionsOnOtherDevices).ToList();
                if (toDeactivate.Any())
                {
                    foreach (var s in toDeactivate)
                    {
                        s.IsActive = false;
                        s.EndedAt = now;
                        s.EndReason = s.DeviceFingerprint == deviceFingerprint ? "Duplicate session cleanup" : "New login from another device.";
                        s.UpdatedAt = now;
                    }
                    await _unitOfWork.SessionRepository.UpdateRangeAsync(toDeactivate);
                }

                // 4. Process Group A (The one to keep/create)
                string finalSessionId;
                if (currentSession != null)
                {
                    currentSession.IsActive = true;
                    currentSession.LastAccessedAt = now;
                    currentSession.ExpiresAt = now.AddMinutes(_sessionSettings.SessionTimeoutMinutes);
                    currentSession.IpAddress = ipAddress;
                    currentSession.UserAgent = userAgent;
                    currentSession.UpdatedAt = now;

                    await _unitOfWork.SessionRepository.UpdateAsync(currentSession);
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
                    await _unitOfWork.SessionRepository.CreateAsync(newSession);
                    finalSessionId = newSession.Id;
                }

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return AppResponse<string>.Success("Session managed successfully", finalSessionId);

            }
            catch (DbUpdateConcurrencyException ex) when (attempt < maxRetries)
            {
                _logger.LogWarning(ex,
                    "Concurrency conflict on attempt {Attempt} of {MaxRetries} for user {UserId}",
                    attempt, maxRetries, userId);

                await _unitOfWork.RollbackTransactionAsync();

                // Clear change tracker to reset state
                _unitOfWork.ClearChangeTracker();

                // Wait before retry with exponential backoff
                await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt));

                // Retry
                continue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating session for user: {UserId}", userId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        // If we exhausted retries
        throw new InvalidOperationException($"Failed to create session for user {userId} after {maxRetries} attempts due to concurrent modifications");
            
    }

    public async Task<AppResponse<bool>> EndSessionAsync(string sessionId)
    {
        try
        {
            var session = await _unitOfWork.SessionRepository.FindByCondition(x => x.Id == sessionId).FirstOrDefaultAsync();
            if (session == null)
            {
                return AppResponse<bool>.Success("Session not found", true);
            }

            session.IsActive = false;
            session.EndedAt = DateTimeOffset.UtcNow;
            session.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SessionRepository.UpdateAsync(session);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Session ended: {SessionId} for user: {UserId}", sessionId, session.UserId);
            return AppResponse<bool>.Success("Session ended successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending session: {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<AppResponse<bool>> EndAllUserSessionsAsync(string userId, string? excludeSessionId = null)
    {
        try
        {
            var activeSessions = await _unitOfWork.SessionRepository.GetActiveSessionsByUserIdAsync(userId);

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

                await _unitOfWork.SessionRepository.UpdateRangeAsync(sessionsToEnd);

                // IMPORTANT: Save changes!
                await _unitOfWork.CompleteAsync();
            }

            _logger.LogInformation("Ended {Count} sessions for user: {UserId}", sessionsToEnd.Count, userId);
            return AppResponse<bool>.Success($"Ended {sessionsToEnd.Count} sessions", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending all sessions for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<AppResponse<List<UserSession>>> GetActiveSessionsAsync(string userId)
    {
        try
        {
            var sessions = await _unitOfWork.SessionRepository.GetActiveSessionsByUserIdAsync(userId);
            var sessionList = sessions.ToList();

            return AppResponse<List<UserSession>>.Success("Active sessions retrieved", sessionList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active sessions for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<AppResponse<bool>> CleanupExpiredSessionsAsync()
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

            return AppResponse<bool>.Success($"Cleaned up {expiredList.Count} expired sessions", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during session cleanup");
            throw;
        }
    }

    public async Task<AppResponse<bool>> IsSessionValidAsync(string sessionId, string userId)
    {
        var session = await _unitOfWork.SessionRepository
                .FindByCondition(s => s.Id == sessionId && s.UserId == userId).FirstOrDefaultAsync() ;

        if (session is null)
            return AppResponse<bool>.Failure("Session not found");

        if (!session.IsActive)
            return AppResponse<bool>.Failure("Session is not active");

        session.LastAccessedAt = DateTimeOffset.UtcNow;
        session.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        await _unitOfWork.CompleteAsync();

        return AppResponse<bool>.Success("Session is valid", true);
    }

    public async Task<AppResponse<bool>> IsSessionValidAsync_(string sessionId, string userId)
    {
        try
        {
            var session = await _unitOfWork.SessionRepository
                .FindByCondition(s => s.Id == sessionId && s.UserId == userId)
                .AsNoTracking() 
                .FirstOrDefaultAsync();

            if (session == null || session.UserId != userId)
            {
                // If not tracked, then and only then, go to the database
                if (session == null)
                {
                    session = await _unitOfWork.SessionRepository
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


                await _unitOfWork.SessionRepository.UpdateAsync(session);
                await _unitOfWork.CompleteAsync();

                return AppResponse<bool>.Failure("Session has expired");
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

            return AppResponse<bool>.Success("Session is valid", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating session: {SessionId}", sessionId);
            throw;
        }
    }


}
