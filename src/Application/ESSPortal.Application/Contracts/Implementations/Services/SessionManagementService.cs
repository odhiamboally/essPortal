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
                // If at limit, end the oldest sessions
                var endSessionsResponse = await EndAllUserSessionsAsync(userId);

                if (!endSessionsResponse.Successful)
                {
                    _logger.LogError("Failed to end old sessions for user {UserId}: {Message}",
                        userId, endSessionsResponse.Message);
                    return ApiResponse<bool>.Failure("Failed to end old sessions");
                }

                _logger.LogInformation("Ended old sessions for user {UserId} due to concurrent limit", userId);
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

            _logger.LogInformation("Session created for user {UserId}: {SessionId}", userId, sessionId);
            return ApiResponse<bool>.Success("Session created successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<ApiResponse<bool>> CreateSessionAsync_(string userId, string sessionId, string ipAddress, string userAgent, string deviceFingerprint)
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

    public async Task<ApiResponse<bool>> CreateSessionAsync(string userId, string sessionId, string ipAddress, string userAgent, string deviceFingerprint)
    {
        const int maxRetries = 3;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Query INSIDE transaction for consistency
                var activeSessions = await _unitOfWork.SessionRepository.GetActiveSessionsByUserIdAsync(userId);

                var activeSessionsList = activeSessions.ToList();

                // Check if session for THIS device exists
                var existingSessionForDevice = activeSessionsList.FirstOrDefault(s => s.DeviceFingerprint == deviceFingerprint);

                // Get sessions to end (exclude current device session)
                var sessionsToEnd = activeSessionsList.Where(s => s.DeviceFingerprint != deviceFingerprint).ToList();
                    
                // End other device sessions
                if (sessionsToEnd.Any())
                {
                    _logger.LogInformation(
                        "User {UserId} signing in from device {DeviceFingerprint}. Ending {Count} concurrent session(s).",
                        userId, deviceFingerprint, sessionsToEnd.Count);

                    foreach (var session in sessionsToEnd)
                    {
                        session.IsActive = false;
                        session.EndedAt = DateTimeOffset.UtcNow;
                        session.EndReason = "New login from another device.";
                        session.UpdatedAt = DateTimeOffset.UtcNow;
                    }

                    await _unitOfWork.SessionRepository.UpdateRangeAsync(sessionsToEnd);
                }

                var now = DateTimeOffset.UtcNow;

                // Update existing session OR create new one (not both)
                if (existingSessionForDevice != null)
                {
                    _logger.LogInformation("Re-authenticating session {SessionId} for user {UserId} on existing device {DeviceFingerprint}",existingSessionForDevice.Id, userId, deviceFingerprint);
                        
                    existingSessionForDevice.LastAccessedAt = now;
                    existingSessionForDevice.ExpiresAt = now.AddMinutes(_sessionSettings.SessionTimeoutMinutes);
                    existingSessionForDevice.IpAddress = ipAddress;
                    existingSessionForDevice.UserAgent = userAgent;
                    existingSessionForDevice.UpdatedAt = now;

                    await _unitOfWork.SessionRepository.UpdateAsync(existingSessionForDevice);
                }
                else
                {
                    // Create new session for new device
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
                }

                // Save all changes
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Session created/updated for user {UserId} on device {DeviceFingerprint}",userId, deviceFingerprint);
                    
                return ApiResponse<bool>.Success("Session created successfully", true);
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
