using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Application.Dtos.Auth;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Profile;
using ESSPortal.Application.Dtos.TwoFactor;
using ESSPortal.Domain.Entities;
using ESSPortal.Domain.Interfaces;
using ESSPortal.Domain.NavEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ESSPortal.Application.Contracts.Implementations.Services;
internal sealed class TwoFactorService(
    IUnitOfWork unitOfWork,
    UserManager<AppUser> userManager,
    ILogger<TwoFactorService> logger,
    IHttpContextAccessor httpContextAccessor,
    ITotpService totpService,
    IEncryptionService encryptionService,
    IJwtService jWtService,
    IClaimsService claimsService,
    INavisionService navisionService)
    : ITwoFactorService
{
    public async Task<ApiResponse<TwoFactorSetupInfo>> GetSetupInfoAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return ApiResponse<TwoFactorSetupInfo>.Failure("User not found.");
            }

            var existingTempSecret = await unitOfWork.TempTotpSecretRepository.GetValidTempSecretByUserIdAsync(userId);

            string secretKey;
            if (existingTempSecret != null)
            {
                secretKey = encryptionService.Decrypt(existingTempSecret.EncryptedSecret);
            }
            else
            {
                secretKey = totpService.GenerateSecret();

                await StoreTemporarySecretAsync(userId, secretKey);
            }

            var qrCodeUri = totpService.GenerateQrCodeUri(user.Email!, secretKey);

            var setupInfo = new TwoFactorSetupInfo
            {
                QrCodeUri = qrCodeUri,
                ManualEntryKey = secretKey
            };

            logger.LogInformation("Generated 2FA setup info for user {UserId}", userId);
            return ApiResponse<TwoFactorSetupInfo>.Success("Setup information generated successfully.", setupInfo);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating 2FA setup info");
            return ApiResponse<TwoFactorSetupInfo>.Failure("An error occurred while generating setup information.");
        }
    }

    public async Task<ApiResponse<bool>> EnableTwoFactorAsync(EnableTwoFactorRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return ApiResponse<bool>.Failure("User not found.");
            }

            var tempSecret = await unitOfWork.TempTotpSecretRepository.GetValidTempSecretByUserIdAsync(userId);
            if (tempSecret == null)
            {
                return ApiResponse<bool>.Failure("Setup session expired. Please start the setup process again.");
            }

            var decrtyptedSecret = encryptionService.Decrypt(tempSecret.EncryptedSecret);

            var isValidCode = totpService.VerifyTotpCode(decrtyptedSecret, request.VerificationCode);

            if (!isValidCode)
            {
                logger.LogWarning("Invalid TOTP code provided for user {UserId}", userId);
                return ApiResponse<bool>.Failure("Invalid verification code. Please try again.");
            }

            await unitOfWork.BeginTransactionAsync();
            try
            {
                await StorePermanentSecretAsync(userId, tempSecret.EncryptedSecret);
                await unitOfWork.TempTotpSecretRepository.DeleteUserTempSecretsAsync(userId);
                await userManager.SetTwoFactorEnabledAsync(user, true);
                await GenerateInitialBackupCodesAsync(userId);
                await unitOfWork.CompleteAsync();

                await unitOfWork.CommitTransactionAsync();

                await unitOfWork.CompleteAsync();
            }
            catch
            {
                await unitOfWork.CommitTransactionAsync();
                throw;
            }

            logger.LogInformation("Two-factor authentication enabled for user {UserId}", userId);
            return ApiResponse<bool>.Success("Two-factor authentication enabled successfully.", true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error enabling 2FA for user");
            return ApiResponse<bool>.Failure("An error occurred while enabling two-factor authentication.");
        }
    }

    public async Task<ApiResponse<bool>> DisableTwoFactorAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return ApiResponse<bool>.Failure("User not found.");
            }

            // Disable 2FA
            await userManager.SetTwoFactorEnabledAsync(user, false);

            // Clear stored secrets and backup codes
            await ClearAllUserSecretsAsync(userId);

            logger.LogInformation("Two-factor authentication disabled for user {UserId}", userId);
            return ApiResponse<bool>.Success("Two-factor authentication disabled successfully.", true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error disabling 2FA for user");
            return ApiResponse<bool>.Failure("An error occurred while disabling two-factor authentication.");
        }
    }

    public async Task<ApiResponse<TwoFactorStatus>> GetTwoFactorStatusAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return ApiResponse<TwoFactorStatus>.Failure("User not found.");
            }

            var twoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user);
            var hasBackupCodes = await HasBackupCodesAsync(userId);

            var status = new TwoFactorStatus
            {
                IsEnabled = twoFactorEnabled,
                HasBackupCodes = hasBackupCodes
            };

            return ApiResponse<TwoFactorStatus>.Success("Status retrieved successfully.", status);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting 2FA status for user");
            return ApiResponse<TwoFactorStatus>.Failure("An error occurred while getting two-factor status.");
        }
    }

    public async Task<ApiResponse<BackupCodesInfo>> GenerateBackupCodesAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return ApiResponse<BackupCodesInfo>.Failure("User not found.");
            }

            var twoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user);
            if (!twoFactorEnabled)
            {
                return ApiResponse<BackupCodesInfo>.Failure("Two-factor authentication must be enabled first.");
            }

            var backupCodes = GenerateBackupCodes();

            await StoreBackupCodesAsync(userId, backupCodes);

            var backupInfo = new BackupCodesInfo
            {
                BackupCodes = backupCodes
            };

            logger.LogInformation("Backup codes generated for user {UserId}", userId);
            return ApiResponse<BackupCodesInfo>.Success("Backup codes generated successfully.", backupInfo);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating backup codes for user");
            return ApiResponse<BackupCodesInfo>.Failure("An error occurred while generating backup codes.");
        }
    }

    public async Task<ApiResponse<Verify2FACodeResponse>> VerifyTotpCodeAsync(VerifyTotpCodeRequest request)
    {
        try
        {
            var userId = string.IsNullOrWhiteSpace(request.UserId) ? GetCurrentUserId() : request.UserId;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return ApiResponse<Verify2FACodeResponse>.Failure("User not found.");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<Verify2FACodeResponse>.Failure("User not found.");
            }

            var twoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user);
            if (!twoFactorEnabled)
            {
                return ApiResponse<Verify2FACodeResponse>.Failure("Two-factor authentication is not enabled.");
            }

            var totpSecret = await unitOfWork.UserTotpSecretRepository.GetActiveSecretByUserIdAsync(userId);
            if (totpSecret == null)
            {
                logger.LogWarning("No active TOTP secret found for user: {UserId}", userId);

                return ApiResponse<Verify2FACodeResponse>.Failure("TOTP not configured. Please set up your authenticator app first.");
            }

            var decryptedSecret = encryptionService.Decrypt(totpSecret.EncryptedSecret);

            var isValidCode = totpService.VerifyTotpCode(decryptedSecret, request.Code);

            if (!isValidCode)
            {
                logger.LogWarning("Invalid TOTP code provided for user: {UserId}", userId);
                return ApiResponse<Verify2FACodeResponse>.Failure("Invalid verification code. Please try again.");
            }

            logger.LogInformation("TOTP code verified successfully for user: {UserId}", userId);

            var tokenResponse = await jWtService.GenerateToken(user);
            if (!tokenResponse.Successful || string.IsNullOrWhiteSpace(tokenResponse.Data))
            {
                logger.LogError("Failed to generate token for user: {UserId}", user.Id);
                return ApiResponse<Verify2FACodeResponse>.Failure("Could not generate authentication token");
            }

            var tokenExpiry = jWtService.GetTokenExpiry(tokenResponse.Data);

            var refreshToken = jWtService.GenerateRefreshToken(user);
            if (!refreshToken.Successful || string.IsNullOrWhiteSpace(refreshToken.Data))
            {
                logger.LogError("Failed to generate refresh token for user: {UserId}", user.Id);
                return ApiResponse<Verify2FACodeResponse>.Failure("Could not generate refresh token");
            }

            var userClaims = await claimsService.GetUserClaimsAsync(user);
            if (!userClaims.Successful || userClaims.Data == null)
            {
                logger.LogError("Failed to get user claims for user: {UserId}", user.Id);
                return ApiResponse<Verify2FACodeResponse>.Failure("Could not retrieve user claims");
            }

            var roles = await userManager.GetRolesAsync(user);

            var emailConfirmed = await userManager.IsEmailConfirmedAsync(user);

            var employeeCardResponse = await navisionService.GetSingleAsync<EmployeeCard>(user.EmployeeNumber!);
            var employeeCard = employeeCardResponse.Data;

            var employeeResponse = await navisionService.GetSingleAsync<Employees>(user.EmployeeNumber!);
            var employee = employeeResponse.Data;

            var userInfo = new UserInfo(
                user.Id,
                employee?.ID_No ?? string.Empty,
                user.EmployeeNumber,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                employeeCard?.Responsibility_Center ?? string.Empty,
                employee?.Job_Position_Title ?? string.Empty,
                employeeCard?.Manager_Supervisor ?? string.Empty,
                employee?.Employment_Type ?? string.Empty,
                user.ProfilePictureUrl,
                employee?.Country_Region_Code,
                emailConfirmed,
                false, //phoneConfirmed,
                twoFactorEnabled,
                user.LastLoginAt,
                roles.ToList()
            );

            var claimResponse = userClaims.Data.Select(claim => new UserClaimsResponse
            {
                Type = claim.Type,
                Value = claim.Value,
                ValueType = claim.ValueType,
                Issuer = claim.Issuer,
                OriginalIssuer = claim.OriginalIssuer,
                Properties = claim.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            }).ToList();

            var response = new Verify2FACodeResponse(
                tokenResponse.Data,
                refreshToken.Data,
                user.Id,
                true,
                tokenExpiry.Data,
                userInfo,
                claimResponse

            );
            
            return ApiResponse<Verify2FACodeResponse>.Success("TOTP code verified successfully.", response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying TOTP code for user: {UserId}", request.UserId);
            return ApiResponse<Verify2FACodeResponse>.Failure("An error occurred while verifying the code.");
        }
    }

    public async Task<ApiResponse<bool>> VerifyBackupCodeAsync(VerifyBackupCodeRequest request)
    {
        try
        {
            var hashedCode = encryptionService.HashCode(request.Code);
            var backupCode = await unitOfWork.UserBackupCodeRepository.GetUnusedCodeByHashAsync(request.UserId, hashedCode);

            if (backupCode == null) return ApiResponse<bool>.Failure("2FA backup code not found");

            // Mark as used
            await unitOfWork.UserBackupCodeRepository.MarkCodeAsUsedAsync(backupCode.Id);
            await unitOfWork.CompleteAsync();

            return ApiResponse<bool>.Success("2FA backup code varified", true);
        }
        catch (Exception)
        {

            throw;
        }
        
    }
    



    private string GetCurrentUserId()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    private List<string> GenerateBackupCodes()
    {
        var codes = new List<string>();
        using var rng = RandomNumberGenerator.Create();

        for (int i = 0; i < 10; i++)
        {
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var code = BitConverter.ToUInt32(bytes, 0).ToString("D8");
            codes.Add($"{code[..4]}-{code[4..]}");
        }

        return codes;
    }

    private async Task GenerateInitialBackupCodesAsync(string userId)
    {
        // Generate and store initial backup codes when 2FA is first enabled
        var codes = GenerateBackupCodes();
        await StoreBackupCodesAsync(userId, codes);
    }

    private async Task<bool> HasBackupCodesAsync(string userId)
    {
        var count = await unitOfWork.UserBackupCodeRepository.GetActiveCodesCountAsync(userId);
        return count > 0;
    }

    private async Task StoreTemporarySecretAsync(string userId, string secret)
    {
        var existingActiveSecrets = await unitOfWork.TempTotpSecretRepository
        .FindByCondition(x => x.UserId == userId &&
                             !x.IsDeleted &&
                             x.ExpiresAt > DateTimeOffset.UtcNow)
        .ToListAsync();

        if (existingActiveSecrets.Any())
        {
            logger.LogInformation("Cleaning up {Count} existing active secrets for user {UserId}",
                existingActiveSecrets.Count, userId);

            foreach (var existing in existingActiveSecrets)
            {
                existing.IsDeleted = true;
                existing.DeletedAt = DateTimeOffset.UtcNow;
                existing.DeletedBy = userId;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
                existing.UpdatedBy = userId;
            }
        }

        var encryptedSecret = encryptionService.Encrypt(secret);

        var tempSecret = new TempTotpSecret
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            EncryptedSecret = encryptedSecret,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId
        };

        await unitOfWork.TempTotpSecretRepository.CreateAsync(tempSecret);
        await unitOfWork.CompleteAsync();
    }

    private async Task StorePermanentSecretAsync(string userId, string secret)
    {
        await unitOfWork.UserTotpSecretRepository.DeactivateUserSecretsAsync(userId);

        var secretEntity = new UserTotpSecret
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            EncryptedSecret = secret,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId
        };

        await unitOfWork.UserTotpSecretRepository.CreateAsync(secretEntity);
        await unitOfWork.CompleteAsync();
    }

    private async Task StoreBackupCodesAsync(string userId, List<string> codes)
    {
        await unitOfWork.UserBackupCodeRepository.DeleteUserCodesAsync(userId);

        var backupCodeEntities = codes.Select(code => new UserBackupCode
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            HashedCode = encryptionService.HashCode(code),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(1), 
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId

        }).ToList();

        foreach (var backupCode in backupCodeEntities)
        {
            await unitOfWork.UserBackupCodeRepository.CreateAsync(backupCode);
            await unitOfWork.CompleteAsync();
        }
    }

    private async Task ClearAllUserSecretsAsync(string userId)
    {
        // Clear TOTP secrets
        await unitOfWork.UserTotpSecretRepository.DeactivateUserSecretsAsync(userId);
        await unitOfWork.TempTotpSecretRepository.DeleteUserTempSecretsAsync(userId);

        // Clear backup codes
        await unitOfWork.UserBackupCodeRepository.DeleteUserCodesAsync(userId);
    }

    
}
