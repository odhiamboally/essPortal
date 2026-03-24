using EssPortal.Domain.Enums.NavEnums;
using EssPortal.Shared.Dtos.Auth;
using EssPortal.Shared.Dtos.ModelFilters;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Application.Dtos.Auth;

using ESSPortal.Application.Extensions;
using ESSPortal.Application.Mappings;
using ESSPortal.Domain.Entities;
using ESSPortal.Domain.Interfaces;
using ESSPortal.Domain.NavEntities;
using ESSPortal.Infrastructure.Configuration;
using ESSPortal.Infrastructure.Utilities;
using ESSPortal.Shared.Contracts.Interfaces.Common;
using ESSPortal.Shared.Dtos.Auth;
using ESSPortal.Shared.Dtos.Common;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using UserProfile = ESSPortal.Domain.Entities.UserProfile;

namespace ESSPortal.Infrastructure.Contracts.Implementations.Services;
internal sealed class AuthService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IHttpContextAccessor contextAccessor,
    INavisionService navisionService,
    IJwtService jwtService,
    ILogger<AuthService> logger,
    IClaimsService claimsService,
    IEmailService emailService,
    IFileService fileService,
    IEmployeeService employeeService,
    IOptions<EmailSettings> emailSettings,
    IUnitOfWork unitOfWork,
    ITotpService totpService,
    IEncryptionService encryptionService,
    ISessionManagementService sessionManagementService,
    ICacheService cacheService

        ) : IAuthService
{
    private readonly UserManager<AppUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    private readonly SignInManager<AppUser> _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
    private readonly IHttpContextAccessor _httpContextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
    private readonly IJwtService _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
    private readonly IClaimsService _claimsService = claimsService ?? throw new ArgumentNullException(nameof(claimsService));
    private readonly ILogger<AuthService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly INavisionService _navisionService = navisionService ?? throw new ArgumentNullException(nameof(navisionService));
    private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    private readonly IFileService _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
    private readonly IEmployeeService _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
    private readonly EmailSettings _emailSettings = emailSettings.Value ?? throw new ArgumentNullException(nameof(emailSettings));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork)); 
    private readonly ITotpService _totpService = totpService ?? throw new ArgumentNullException(nameof(totpService)); 
    private readonly IEncryptionService _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
    private readonly ISessionManagementService _sessionManagementService = sessionManagementService ?? throw new ArgumentNullException(nameof(sessionManagementService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));

    public async Task<AppResponse<bool>> RegisterEmployeeAsync(RegisterEmployeeRequest request)
    {
        AppUser? createdUser = null;

        try
        {
            var result = await _unitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
            {
                var employeeData = await ValidateEmployeeInBusinessCentralAsync(request.EmployeeNumber ?? string.Empty);
                if (employeeData == null)
                {
                    return AppResponse<bool>.Failure("Invalid employee number. Please contact HR.");
                }

                var existingUser = await _userManager.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.EmployeeNumber == request.EmployeeNumber);

                if (existingUser != null)
                {
                    return AppResponse<bool>.Failure("You are already registered. Please log in.");
                }

                var enrichedRequest = new RegisterEmployeeRequest
                {
                    EmployeeNumber = employeeData.EmployeeNumber,
                    FirstName = employeeData.FirstName,
                    MiddleName = employeeData.MiddleName,
                    LastName = employeeData.LastName,
                    Email = employeeData.Email,
                    PhoneNumber = employeeData.PhoneNumber,
                    Gender = employeeData.Gender,
                    Password = employeeData.Password,
                    ConfirmPassword = employeeData.ConfirmPassword,
                    IsActive = true,
                    IsDeleted = false,
                    ReturnUrl = request.ReturnUrl,
                    ExternalLogins = request.ExternalLogins,
                };

                var appUser = enrichedRequest.ToAppUser();

                var createResult = await _userManager.CreateAsync(appUser, request.Password ?? string.Empty);
                if (!createResult.Succeeded)
                {
                    return AppResponse<bool>.Failure("Account creation failed.");
                }

                createdUser = appUser;

                var profile = new UserProfile
                {
                    UserId = appUser.Id,
                    CreatedBy = appUser.Id,
                    UpdatedBy = appUser.Id,
                    TelephoneNo = appUser.PhoneNumber,
                    MobileNo = employeeData.PhoneNumber,
                    ContactEMailAddress = employeeData.Email
                };

                await _unitOfWork.UserProfileRepository.CreateAsync(profile);

                return AppResponse<bool>.Success("Account created successfully", true);
            });

            // 🔥 OUTSIDE transaction (important)
            if (result.Successful && createdUser != null)
            {
                var emailSent = await SendRegistrationConfirmationEmailAsync(createdUser!);
                if (!emailSent)
                {
                    _logger.LogWarning("User created but email failed for {UserId}", createdUser?.Id);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for {EmployeeNumber}", request.EmployeeNumber);

            // Cleanup Identity if needed
            if (createdUser != null)
            {
                await _userManager.DeleteAsync(createdUser);
            }

            throw;
        }
    }

    public async Task<AppResponse<bool>> SendEmailConfirmationAsync(SendEmailConfirmationRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Email confirmation requested for non-existent email: {Email}", request.Email);
                return AppResponse<bool>.Success("If the email exists, a confirmation link has been sent", true);
            }

            if (user.EmailConfirmed)
            {
                return AppResponse<bool>.Success("Email is already confirmed", true);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmationUrl = $"{_emailSettings.ClientBaseUrl}/Auth/ConfirmEmail?email={Uri.EscapeDataString(request.Email)}&token={Uri.EscapeDataString(token)}";

            var emailBody = $@"
            <h2>Confirm Your Email Address</h2>
            <p>Hello {user.FirstName ?? "there"},</p>
            <p>Please confirm your email address by clicking the link below:</p>
            <p><a href='{confirmationUrl}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Confirm Email</a></p>
            <p>If you didn't create an account, you can safely ignore this email.</p>
            <p>This link will expire in 24 hours.</p>
        ";

            await _emailService.SendEmailAsync(new SendEmailRequest
            {
                To = user.Email!,
                Subject = "Confirm Your Email Address",
                Body = emailBody
            });

            return AppResponse<bool>.Success("Confirmation email sent", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email confirmation to: {Email}", request.Email);
            throw;
        }
    }

    public async Task<AppResponse<bool>> ResendEmailConfirmationAsync(SendEmailConfirmationRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal if email exists for security
                _logger.LogWarning("Confirmation resend requested for non-existent email: {Email}", request.Email);
                return AppResponse<bool>.Success("If the email exists and is unconfirmed, a new confirmation link has been sent.", true);
            }

            if (user.EmailConfirmed)
            {
                return AppResponse<bool>.Success("Email is already confirmed. You can sign in now.", true);
            }

            await SendRegistrationConfirmationEmailAsync(user);
            return AppResponse<bool>.Success("Confirmation email has been resent.", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending confirmation email to: {Email}", request.Email);
            throw;
        }
    }

    public async Task<AppResponse<bool>> ConfirmUserEmailAsync(ConfirmUserEmailRequest confirmEmailRequest)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(confirmEmailRequest.Email);
            if (user == null)
            {
                _logger.LogWarning("Email confirmation attempted for non-existent email: {Email}", confirmEmailRequest.Email);
                return AppResponse<bool>.Failure("Invalid confirmation link");
            }

            if (user.EmailConfirmed)
            {
                return AppResponse<bool>.Success("Email is already confirmed", true);
            }

            var result = await _userManager.ConfirmEmailAsync(user, confirmEmailRequest.Token);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Email confirmation failed for user: {UserId}. Errors: {Errors}",
                    user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return AppResponse<bool>.Failure("Invalid or expired confirmation link");
            }

            return AppResponse<bool>.Success("Email confirmed successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email for request: {Email}", confirmEmailRequest.Email);
            throw;
        }
    }

    public async Task<AppResponse<LoginResponse>> SignInAsync(LoginRequest loginRequest)
    {
        try
        {
            var user = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(u => u.EmployeeNumber == loginRequest.EmployeeNumber);

            if (user == null)
            {
                _logger.LogWarning("Login attempt with invalid employee number: {EmployeeNumber}", loginRequest.EmployeeNumber);
                return AppResponse<LoginResponse>.Failure("Invalid Employee Number or password.");
            }

            // Check if email is confirmed
            var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            if (!emailConfirmed)
            {
                _logger.LogWarning("Login attempt with unconfirmed email for user: {UserId}", user.Id);
                return AppResponse<LoginResponse>.Failure("Please confirm your email before logging in.");
            }

            var signInResult = await _signInManager.PasswordSignInAsync(
                user.UserName,  
                loginRequest.Password,
                loginRequest.RememberMe,
                lockoutOnFailure: true
            );

            if (signInResult.IsLockedOut)
            {
                _logger.LogWarning("Account locked for user: {UserId}", user.Id);
                return AppResponse<LoginResponse>.Failure("Your account is locked due to multiple failed login attempts. Please reset your password or contact support.");
            }

            if (signInResult.IsNotAllowed)
            {
                _logger.LogWarning("Sign in not allowed for user: {UserId}", user.Id);
                return AppResponse<LoginResponse>.Failure("Sign in not allowed. Please contact support.");
            }

            // Check if 2FA is required
            var twoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (!twoFactorEnabled)
            {
                _logger.LogInformation("User {UserId} does not have 2FA enabled", user.Id);
            }

            EmployeeCardFilter employeeCardFilter = new EmployeeCardFilter
            {
                No = user.EmployeeNumber
            };

            var employeeCardResponse = await _employeeService.SearchEmployeeCardsAsync(employeeCardFilter);
            var employeeCard = employeeCardResponse.Data?.Items.FirstOrDefault();

            EmployeesFilter employeeFilter = new EmployeesFilter
            {
                No = user.EmployeeNumber
            };

            var employeeResponse = await _employeeService.SearchEmployeesAsync(employeeFilter);
            var employee = employeeResponse.Data?.Items.FirstOrDefault();

            var userInfo = new UserInfo(
                user.Id,
                employee?.IdNo ?? string.Empty,
                user.EmployeeNumber,
                user.FirstName,
                user.LastName,
                user.Gender,
                user.Email,
                user.PhoneNumber,
                employeeCard?.ResponsibilityCenter ?? string.Empty,
                employee?.JobPositionTitle ?? string.Empty,
                employeeCard?.ManagerSupervisor ?? string.Empty,
                employee?.EmploymentType ?? string.Empty,
                user.ProfilePictureUrl,
                employee?.CountryRegionCode,
                emailConfirmed,
                false,
                twoFactorEnabled,
                user.LastLoginAt,
                []
            );

            if (signInResult.RequiresTwoFactor || twoFactorEnabled)
            {
                // Don't generate full token yet, just a temporary one for 2FA flow
                var tempClaims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id),
                    new(ClaimTypes.Name, user.UserName!),
                    new("temp_auth", "true")
                };

                var tempToken = _jwtService.GenerateTemporaryToken(tempClaims, TimeSpan.FromMinutes(10));
                if (!tempToken.Successful || string.IsNullOrWhiteSpace(tempToken.Data))
                {
                    _logger.LogError("Failed to generate temporary token for user: {UserId}", user.Id);
                    return AppResponse<LoginResponse>.Failure("Could not generate temporary authentication token");
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userInfoWith2FA = userInfo with { Roles = roles.ToList() };

                _cacheService.SetUserInfo(user.EmployeeNumber ?? string.Empty, userInfoWith2FA);

                return AppResponse<LoginResponse>.Success("Two-factor authentication required", new LoginResponse(
                    user.Id ?? string.Empty,
                    user.EmployeeNumber ?? string.Empty,
                    user.FirstName ?? string.Empty,
                    user.LastName ?? string.Empty,
                    user.Email ?? string.Empty,
                    true,
                    false,
                    false,
                    tempToken.Data,
                    string.Empty,
                    DateTimeOffset.UtcNow.AddMinutes(10),
                    userInfoWith2FA,
                    tempClaims.ToDtoList())
                );
            }

            if (!signInResult.Succeeded)
            {
                _logger.LogWarning("Sign in failed for user: {UserId}", user.Id);
                return AppResponse<LoginResponse>.Failure("Invalid login attempt.");
            }

            var sessionId = Guid.CreateVersion7().ToString();
            var sessionCreationResult = await _sessionManagementService.CreateOrUpdateSessionAsync(
                user.Id,
                sessionId,
                _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown",
                _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "unknown",
                loginRequest.DeviceFingerprint ?? "unknown"
            );

            if (!sessionCreationResult.Successful)
            {
                return AppResponse<LoginResponse>.Failure("Could not establish a user session.");
            }

            if(sessionId != sessionCreationResult.Data)
                sessionId = sessionCreationResult.Data;

            var userClaims = await _claimsService.GetUserClaimsAsync(user);
            if (!userClaims.Successful || userClaims.Data == null || userClaims.Data.Count == 0)
            {
                return AppResponse<LoginResponse>.Failure("Could not retrieve user claims");
            }

            var tokenResponse = await _jwtService.GenerateToken(user);
            if (!tokenResponse.Successful || string.IsNullOrWhiteSpace(tokenResponse.Data))
            {
                return AppResponse<LoginResponse>.Failure("Could not generate authentication token");
            }

            var refreshToken = _jwtService.GenerateRefreshToken(user);
            if (!refreshToken.Successful || string.IsNullOrWhiteSpace(refreshToken.Data))
            {
                return AppResponse<LoginResponse>.Failure("Could not generate refresh token");
            }

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwt = jwtHandler.ReadJwtToken(tokenResponse.Data);
            var tokenExpiry = jwt.ValidTo;

            var tokenValidationResponse = _jwtService.IsTokenValid(tokenResponse.Data);
            if (!tokenValidationResponse.Successful || !tokenValidationResponse.Data)
            {
                return AppResponse<LoginResponse>.Failure("Invalid authentication token");
            }

            var isSecurityTokenValid = tokenValidationResponse.Data;

            if (!isSecurityTokenValid)
            {
                throw new SecurityTokenValidationException($"Error|Token is Invalid");
            }

            var rolesResponse = await _userManager.GetRolesAsync(user);
            var userRoles = rolesResponse.ToList();
            var finalUserInfo = userInfo with { Roles = userRoles };

            _cacheService.SetUserInfo(user.EmployeeNumber ?? string.Empty, finalUserInfo);

            await _userManager.Users
                .Where(u => u.Id == user.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.LastLoginAt, DateTimeOffset.UtcNow));

            return AppResponse<LoginResponse>.Success("Login successful", new LoginResponse(
                user.Id ?? string.Empty,
                user.EmployeeNumber ?? string.Empty,
                user.FirstName ?? string.Empty,
                user.LastName ?? string.Empty,
                user.Email ?? string.Empty,
                false,
                false,
                true,
                tokenResponse.Data,
                refreshToken.Data ?? string.Empty,
                tokenExpiry,
                finalUserInfo,
                userClaims.Data.ToDtoList())

            ) with { SessionId = sessionId };
            
            

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for employee number: {EmployeeNumber}", loginRequest.EmployeeNumber);
            throw;
        }
    }

    public async Task<AppResponse<CurrentUserResponse>> GetCurrentUserAsync()
    {
        try
        {
            var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(userName))
                return AppResponse<CurrentUserResponse>.Failure("User not authenticated");

            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return AppResponse<CurrentUserResponse>.Failure("User not found.");

            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null)
                return AppResponse<CurrentUserResponse>.Failure("User not found.");

            // Get user roles
            var roles = await _userManager.GetRolesAsync(appUser);
            var rolesList = roles.ToList();

            var twoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(appUser);

            var emailConfirmed = await _userManager.IsEmailConfirmedAsync(appUser);

            await _userManager.IsPhoneNumberConfirmedAsync(appUser);

            // Get last login time (you might store this in your ApplicationUser entity)
            var lastLoginClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("LastLogin");

            DateTimeOffset? lastLoginAt = null;
            if (lastLoginClaim?.Value != null && DateTimeOffset.TryParse(lastLoginClaim.Value, out var lastLogin))
            {
                lastLoginAt = lastLogin;
            }

            bool? isAuthenticated = _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

            return AppResponse<CurrentUserResponse>.Success("CurrentUser", new CurrentUserResponse(
                appUser.EmployeeNumber ?? string.Empty,
                userId,
                appUser.UserName ?? string.Empty,
                appUser.Email ?? string.Empty,
                appUser.FirstName ?? string.Empty,
                appUser.LastName ?? string.Empty,
                appUser.PhoneNumber ?? string.Empty,
                emailConfirmed,
                twoFactorEnabled,
                appUser.Gender!,
                isAuthenticated,
                lastLoginAt,
                rolesList)); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user");
            throw;
        }
    }

    public async Task<AppResponse<ProviderResponse>> Get2FAProvidersAsync(Get2FAProviderRequest providerRequest)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(providerRequest.UserId);
            if (user == null)
                return AppResponse<ProviderResponse>.Failure("User not found.");

            await _userManager.GetValidTwoFactorProvidersAsync(user);

            var hasTotpConfigured = await HasAuthenticatorConfiguredAsync(user);

            var providers = new List<TwoFactorProvider>();

            // Always add Authenticator as an option (even if not yet configured)
            providers.Add(new TwoFactorProvider(
                Value: "Authenticator",
                Text: "Authenticator App",
                DisplayName: "Authenticator App",
                Icon: "shield-lock",
                IsEnabled: hasTotpConfigured,
                Selected: hasTotpConfigured,
                IsDefault: hasTotpConfigured,
                MaskedDestination: hasTotpConfigured ? "Configured" : "Not configured"

            ));
            
            // Ensure at least one provider is selected
            if (!providers.Any(p => p.Selected) && providers.Any())
            {
                providers[0] = providers[0] with { Selected = true, IsDefault = true };
            }

            var response = new ProviderResponse(
                Providers: providers,
                PreferredProvider: providers.FirstOrDefault(p => p.IsDefault)?.Value ?? "Email"
            );

            return AppResponse<ProviderResponse>.Success("Providers retrieved", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting 2FA providers for user: {UserId}", providerRequest.UserId);
            throw;
        }
    }

    public async Task<AppResponse<Send2FACodeResponse>> Send2FACodeAsync(Send2FACodeRequest sendCodeRequest)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(sendCodeRequest.UserId);
            if (user == null)
            {
                _logger.LogWarning("2FA code send attempt for non-existent user: {UserId}", sendCodeRequest.UserId);
                return AppResponse<Send2FACodeResponse>.Failure("User not found");
            }

            var validProviders = await _userManager.GetValidTwoFactorProvidersAsync(user);
            if (!validProviders.Contains(sendCodeRequest.SelectedProvider ?? string.Empty))
                return AppResponse<Send2FACodeResponse>.Failure("Invalid provider. Choose 'Email' or 'Phone'.");

            var token = await _userManager.GenerateTwoFactorTokenAsync(user, sendCodeRequest.SelectedProvider ?? string.Empty);
            if (string.IsNullOrWhiteSpace(token))
                return AppResponse<Send2FACodeResponse>.Failure("Failed to generate authentication token.");

            if (sendCodeRequest.SelectedProvider == "Email")
            {
                await _emailService.SendEmailAsync(new SendEmailRequest
                {
                    To = user.Email,
                    Subject = "Your Two-Factor Authentication Code",
                    Body = $"Your 2FA code is: {token}"
                });
            }

            return AppResponse<Send2FACodeResponse>.Success("2FA code sent successfully.",
                new Send2FACodeResponse
                {
                    UserId = user.Id,
                    Provider = sendCodeRequest.SelectedProvider ?? string.Empty,
                    SelectedProvider = sendCodeRequest.SelectedProvider ?? validProviders.FirstOrDefault()!,
                    MaskedDestination = string.Empty,
                    Token = token, 
                    SentAt = DateTimeOffset.UtcNow,
                    ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
                    CodeLength = 0,
                    CanResend = true, 
                    ResendCooldown = TimeSpan.FromSeconds(30),
                    ReturnUrl = string.Empty

                });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending 2FA code for user: {UserId}", sendCodeRequest.UserId);
            throw;
        }
    }

    public async Task<AppResponse<Verify2FACodeResponse>> Verify2FACodeAsync(Verify2FACodeRequest verifyCodeRequest)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(verifyCodeRequest.UserId);
            if (user == null)
            {
                _logger.LogWarning("2FA verification attempt for non-existent user: {UserId}", verifyCodeRequest.UserId);
                return AppResponse<Verify2FACodeResponse>.Failure("User not found");
            }

            bool isValidCode = false;
            string decryptedSecret = string.Empty;

            if (IsTotpProvider(verifyCodeRequest.Provider))
            {
                var tempSecret = await _unitOfWork.TempTotpSecretRepository.GetValidTempSecretByUserIdAsync(user.Id);

                if (tempSecret != null)
                {
                    decryptedSecret = _encryptionService.Decrypt(tempSecret.EncryptedSecret);

                    isValidCode = _totpService.VerifyTotpCode(decryptedSecret, verifyCodeRequest.Code);

                    if (isValidCode)
                    {
                        await MoveTempSecretToPermanentAsync(user.Id, tempSecret);

                        await _userManager.SetTwoFactorEnabledAsync(user, true);

                    }
                }
                else
                {
                    var totpSecret = await _unitOfWork.UserTotpSecretRepository.GetActiveSecretByUserIdAsync(user.Id);

                    if (totpSecret == null)
                    {
                        return AppResponse<Verify2FACodeResponse>.Failure("Authenticator app not configured. Please set it up first.");
                    }

                    decryptedSecret = _encryptionService.Decrypt(totpSecret.EncryptedSecret);

                    isValidCode = _totpService.VerifyTotpCode(decryptedSecret, verifyCodeRequest.Code);
                }

                if (!isValidCode)
                {
                    return AppResponse<Verify2FACodeResponse>.Failure("Invalid verification code. Please try again.");
                }
            }
            else
            {
                // For Email/SMS providers, use the standard Identity verification
                isValidCode = await _userManager.VerifyTwoFactorTokenAsync(
                    user,
                    verifyCodeRequest.Provider,
                    verifyCodeRequest.Code
                );

                if (!isValidCode)
                {
                    return AppResponse<Verify2FACodeResponse>.Failure("Invalid verification code");
                }
            }

            // Rest of the method remains the same...
            var userClaims = await _claimsService.GetUserClaimsAsync(user);
            if (!userClaims.Successful || userClaims.Data == null)
            {
                return AppResponse<Verify2FACodeResponse>.Failure("Could not retrieve user claims");
            }

            var tokenResponse = await _jwtService.GenerateToken(user);
            if (!tokenResponse.Successful || string.IsNullOrWhiteSpace(tokenResponse.Data))
            {
                return AppResponse<Verify2FACodeResponse>.Failure("Could not generate authentication token");
            }

            var tokenExpiry = _jwtService.GetTokenExpiry(tokenResponse.Data);
            var refreshToken = _jwtService.GenerateRefreshToken(user);

            var sessionId = Guid.CreateVersion7().ToString();
            var sessionCreationResult = await _sessionManagementService.CreateOrUpdateSessionAsync(
                user.Id,
                sessionId,
                _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown",
                _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "unknown",
                verifyCodeRequest.DeviceFingerprint ?? "unknown" 
            );

            if (!sessionCreationResult.Successful)
            {
                return AppResponse<Verify2FACodeResponse>.Failure("Could not establish a user session.");
            }

            user.LastLoginAt = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(user);

            if (verifyCodeRequest.RememberDevice)
            {
                await _signInManager.RememberTwoFactorClientAsync(user);
            }

            await _signInManager.SignInAsync(user, verifyCodeRequest.RememberMe);

            var roles = await _userManager.GetRolesAsync(user);

            var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            await _userManager.IsPhoneNumberConfirmedAsync(user);
            var twoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

            var employeeCardResponse = await _navisionService.GetSingleAsync<EmployeeCard>(user.EmployeeNumber!);
            var employeeCard = employeeCardResponse.Data;

            var employeeResponse = await _navisionService.GetSingleAsync<Employees>(user.EmployeeNumber!);
            var employee = employeeResponse.Data;

            var userInfo = new UserInfo(
                user.Id,
                employee?.ID_No ?? string.Empty,
                user.EmployeeNumber,
                user.FirstName,
                user.LastName,
                user.Gender,
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

            var claimsResponse = userClaims.Data.Select(claim => new UserClaimsResponse
            {
                Type = claim.Type,
                Value = claim.Value,
                ValueType = claim.ValueType,
                Issuer = claim.Issuer,
                OriginalIssuer = claim.OriginalIssuer,
                Properties = claim.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)

            }).ToList();

            return AppResponse<Verify2FACodeResponse>.Success("Two-factor authentication successful",
                new Verify2FACodeResponse(
                    tokenResponse.Data,
                    refreshToken.Data ?? string.Empty,
                    user.Id,
                    true,
                    tokenExpiry.Data,
                    userInfo, // Build this as in your existing code
                    claimsResponse // Build this as in your existing code

                )) with { SessionId = sessionId };
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during 2FA verification for user: {UserId}", verifyCodeRequest.UserId);
            throw;
        }
    }

    public async Task<AppResponse<bool>> RequestPasswordResetAsync(ForgotPasswordRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            // Always return success to prevent email enumeration attacks
            if (user == null || !user.EmailConfirmed)
            {
                _logger.LogWarning("Password reset requested for non-existent or unconfirmed email: {Email}", request.Email);
                return AppResponse<bool>.Success("If the email exists, a reset link has been sent", true);
            }

            if (!user.IsActive || user.IsDeleted)
            {
                _logger.LogWarning("Password reset requested for inactive user: {UserId}", user.Id);
                return AppResponse<bool>.Success("If the email exists, a reset link has been sent", true);
            }

            // Check for rate limiting - prevent spam requests
            var lastResetRequest = await GetLastPasswordResetRequestAsync(user.Id);
            if (lastResetRequest.HasValue && lastResetRequest.Value.AddMinutes(5) > DateTimeOffset.UtcNow)
            {
                _logger.LogWarning("Password reset rate limit exceeded for user: {UserId}", user.Id);
                return AppResponse<bool>.Success("If the email exists, a reset link has been sent", true);
            }

            await RecordPasswordResetRequestAsync(user.Id);

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            if (string.IsNullOrWhiteSpace(resetToken))
            {
                _logger.LogError("Failed to generate password reset token for user: {UserId}", user.Id);
                return AppResponse<bool>.Success("If the email exists, a reset link has been sent", true);
            }

            var resetUrl = 
                $"{_emailSettings.ClientBaseUrl}Auth/ResetPassword?" +
                $"email={Uri.EscapeDataString(user.Email!)}&" +
                $"token={Uri.EscapeDataString(resetToken)}";

            string logoUrl = string.Empty;
            if (!string.IsNullOrWhiteSpace(request.LogoBase64))
            {
                var logoResult = await _fileService.SaveLogoAsync(request.LogoBase64);
                if (logoResult.Successful)
                {
                    logoUrl = logoResult.Data ?? string.Empty;
                }
            }

            var emailBody = $@"
                <h2>Password Reset Request</h2>
                <p>Hello {user.FirstName ?? "there"},</p>
                <p>You requested a password reset for your employee account. Click the link below to reset your password:</p>
                <p><a href='{resetUrl}' style='background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a></p>
                <p><strong>Important:</strong></p>
                <ul>
                    <li>This link will expire in 1 hour</li>
                    <li>If you didn't request this reset, you can safely ignore this email</li>
                    <li>For security, never share this link with anyone</li>
                </ul>
                <p>If you continue to have problems, contact IT support.</p>
            ";

            EmailTemplates.GetPasswordResetEmailTemplate(
                user.FirstName ?? "Unknown",
                resetUrl,
                logoUrl
            );

            await _emailService.SendEmailAsync(new SendEmailRequest
            {
                To = user.Email!,
                Subject = "Password Reset Request",
                Body = emailBody
            });

            return AppResponse<bool>.Success("If the email exists, a reset link has been sent", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting password reset for: {Email}", request.Email);
            throw;
        }
    }

    public async Task<AppResponse<bool>> ValidatePasswordResetTokenAsync(ValidateResetTokenRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Token))
            {
                return AppResponse<bool>.Failure("Invalid reset request.");
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Password reset validation attempted for non-existent email: {Email}", request.Email);
                return AppResponse<bool>.Failure("Invalid or expired reset link");
            }

            var isValidToken = await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword",request.Token);
            
            if (!isValidToken)
            {
                return AppResponse<bool>.Failure("Invalid or expired reset link");
            }

            return AppResponse<bool>.Success("Reset link is valid", true);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating password reset token for: {Email}", request.Email);
            throw;
        }
    }

    public async Task<AppResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        AppUser? user = null;

        try
        {
            var result = await _unitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
            {
                user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return AppResponse<bool>.Failure("Invalid reset request");
                }

                var isValidToken = await _userManager.VerifyUserTokenAsync(
                    user,
                    _userManager.Options.Tokens.PasswordResetTokenProvider,
                    "ResetPassword",
                    request.Token);

                if (!isValidToken)
                {
                    return AppResponse<bool>.Failure("Invalid or expired reset link");
                }

                var password = request.Password ?? request.NewPassword ?? string.Empty;

                if (!IsPasswordStrong(password))
                {
                    return AppResponse<bool>.Failure("Password does not meet requirements");
                }

                if (await _userManager.CheckPasswordAsync(user, password))
                {
                    return AppResponse<bool>.Failure("New password must be different");
                }

                var resetResult = await _userManager.ResetPasswordAsync(user, request.Token, password);
                if (!resetResult.Succeeded)
                {
                    return AppResponse<bool>.Failure("Password reset failed");
                }

                user.ResetFailedLoginAttempts();
                user.PasswordLastChanged = DateTimeOffset.UtcNow;
                user.RequirePasswordChange = false;
                user.UpdatedAt = DateTimeOffset.UtcNow;

                await _userManager.UpdateAsync(user);

                var tokens = await _unitOfWork.TokenRepository.GetActiveTokensByUserIdAsync(user.Id);
                if (tokens.Any())
                {
                    var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                    await _unitOfWork.TokenRepository.RevokeTokensAsync(tokens, "Password reset", ip);
                }

                return AppResponse<bool>.Success("Password reset successful", true);
            });

            // 🔥 OUTSIDE transaction
            if (result.Successful && user != null)
            {
                await SendPasswordResetConfirmationEmailAsync(user);
                await ClearUserCacheAsync(user.Id);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password reset failed for {Email}", request.Email);
            throw;
        }
    }

    public async Task<AppResponse<bool>> VerifyPasswordAsync(VerifyPasswordRequest verifyPasswordRequest)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(verifyPasswordRequest.UserId);
            var userByEmail = await _userManager.FindByEmailAsync(verifyPasswordRequest.Email);

            if (user == null && userByEmail != null)
            {
                user = userByEmail;
            }

            if (user == null)
            {
                return AppResponse<bool>.Failure("User not found");
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return AppResponse<bool>.Failure("Account is locked");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, verifyPasswordRequest.Password);
            if (!isPasswordValid)
            {
                // Increment failed access count
                await _userManager.AccessFailedAsync(user);

                // Check if now locked
                if (await _userManager.IsLockedOutAsync(user))
                {
                    return AppResponse<bool>.Failure("Account locked due to too many failed attempts");
                }

                return AppResponse<bool>.Failure("Invalid password");
            }

            // Reset failed access count on successful verification
            await _userManager.ResetAccessFailedCountAsync(user);

            return AppResponse<bool>.Success("Password verified successfully", true);

        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<AppResponse<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            // Validate the access token format (don't check expiry, it might be expired)
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null || principal.Data == null)
            {
                return AppResponse<RefreshTokenResponse>.Failure("Invalid access token format");
            }

            // Get user ID from the token
            var userId = principal.Data.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return AppResponse<RefreshTokenResponse>.Failure("Invalid token: No user ID found");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return AppResponse<RefreshTokenResponse>.Failure("User not found");
            }

            if (!user.IsActive || user.IsDeleted)
            {
                return AppResponse<RefreshTokenResponse>.Failure("User account is not active");
            }

            // Validate the refresh token
            var storedRefreshToken = await _unitOfWork.TokenRepository.GetRefreshTokenAsync(request.RefreshToken, userId);
            if (storedRefreshToken == null)
            {
                return AppResponse<RefreshTokenResponse>.Failure("Invalid refresh token");
            }

            if (storedRefreshToken.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                await _unitOfWork.TokenRepository.RevokeRefreshTokenAsync(storedRefreshToken, "Token expired");
                return AppResponse<RefreshTokenResponse>.Failure("Refresh token has expired");
            }

            if (storedRefreshToken.IsRevoked)
            {
                _logger.LogWarning("Revoked refresh token used for user: {UserId}", userId);
                return AppResponse<RefreshTokenResponse>.Failure("Refresh token has been revoked");
            }

            // Check if refresh token is used (prevent reuse)
            if (storedRefreshToken.IsUsed)
            {
                _logger.LogWarning("Already used refresh token attempted for user: {UserId}", userId);
                await _unitOfWork.TokenRepository.RevokeAllUserTokensAsync(userId, "Token reuse detected");
                return AppResponse<RefreshTokenResponse>.Failure("Refresh token has already been used");
            }

            // Validate that the refresh token belongs to the same user as the access token
            if (storedRefreshToken.UserId != userId)
            {
                _logger.LogWarning("Refresh token user mismatch. " +
                    "Token UserId: {TokenUserId}, Request UserId: {RequestUserId}",
                    storedRefreshToken.UserId, userId);

                return AppResponse<RefreshTokenResponse>.Failure("Token mismatch");
            }

            // Generate new tokens
            var userClaims = await _claimsService.GetUserClaimsAsync(user);
            if (!userClaims.Successful || userClaims.Data == null)
            {
                return AppResponse<RefreshTokenResponse>.Failure("Could not retrieve user claims");
            }

            var newAccessTokenResponse = await _jwtService.GenerateToken(user);
            if (!newAccessTokenResponse.Successful || string.IsNullOrWhiteSpace(newAccessTokenResponse.Data))
            {
                return AppResponse<RefreshTokenResponse>.Failure("Could not generate new access token");
            }

            var newRefreshTokenResponse = _jwtService.GenerateRefreshToken(user);
            if (!newRefreshTokenResponse.Successful || string.IsNullOrWhiteSpace(newRefreshTokenResponse.Data))
            {
                return AppResponse<RefreshTokenResponse>.Failure("Could not generate new refresh token");
            }

            var tokenExpiry = _jwtService.GetTokenExpiry(newAccessTokenResponse.Data);
            if (!tokenExpiry.Successful)
            {
                return AppResponse<RefreshTokenResponse>.Failure("Could not determine token expiry");
            }

            // Mark old refresh token as used
            await _unitOfWork.TokenRepository.MarkTokenAsUsedAsync(storedRefreshToken);

            // Store new refresh token
            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshTokenResponse.Data,
                UserId = userId,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15),
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedByIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
            };

            await _unitOfWork.TokenRepository.AddRefreshTokenAsync(newRefreshTokenEntity);

            // Clean up old expired tokens for this user (housekeeping)
            await _unitOfWork.TokenRepository.CleanupExpiredTokensAsync(userId);

            // Update user's last activity
            user.UpdatedAt = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(user);

            // Build user profile
            var roles = await _userManager.GetRolesAsync(user);
            var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            await _userManager.IsPhoneNumberConfirmedAsync(user);
            var twoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

            var employeeCardResponse = await _navisionService.GetSingleAsync<EmployeeCard>(user.EmployeeNumber!);
            var employeeCard = employeeCardResponse.Data;

            var employeeResponse = await _navisionService.GetSingleAsync<Employees>(user.EmployeeNumber!);
            var employee = employeeResponse.Data;

            var userInfo = new UserInfo(
                user.Id,
                employee?.ID_No ?? string.Empty,
                user.EmployeeNumber,
                user.FirstName,
                user.LastName,
                user.Gender,
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
                [.. roles]
            );

            return AppResponse<RefreshTokenResponse>.Success("Token refreshed successfully", new RefreshTokenResponse(
                newAccessTokenResponse.Data,
                newRefreshTokenResponse.Data,
                userId,
                tokenExpiry.Data,
                tokenExpiry.Data,
                userInfo,
                userClaims.Data

            ));

        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<AppResponse<bool>> SignOutAsync()
    {
        try
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            // Try to get session ID from headers
            var sessionId = _httpContextAccessor.HttpContext?.Request.Headers["X-Session-Id"].FirstOrDefault();

            _logger.LogInformation("API sign out called. UserId: {UserId}, SessionId: {SessionId}", userId, sessionId);

            await _signInManager.SignOutAsync();

            if (!string.IsNullOrWhiteSpace(userId))
            {
                await CleanupUserSessionAsync(userId, "API Logout");

                // If we have a session ID, we could do additional cleanup
                if (!string.IsNullOrWhiteSpace(sessionId))
                {
                    // End specific session if you're tracking sessions
                    await EndSessionAsync(sessionId);
                    _logger.LogInformation("Session ended: {SessionId} for user: {UserId}", sessionId, userId);
                }

                _logger.LogInformation("API sign out completed for user: {UserId}", userId);
            }

            return AppResponse<bool>.Success("Signed out successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during API sign out");
            throw;
        }
    }


    private async Task<RegisterEmployeeRequest?> ValidateEmployeeInBusinessCentralAsync(string employeeNumber)
    {
        try
        {
            var employeesFilter = new EmployeesFilter { No = employeeNumber };
            var employeeCardFilter = new EmployeeCardFilter { No = employeeNumber };

            employeeCardFilter.BuildODataFilter();

            var exisitingEmployeeResponse = await _employeeService.GetEmployeeByNoAsync(employeeNumber);
            if (!exisitingEmployeeResponse.Successful || exisitingEmployeeResponse.Data == null)
            {
                _logger.LogWarning("Employee not found in Business Central: {EmployeeNumber}", employeeNumber);
                return null;
            }

            var employeeDetails = await _employeeService.SearchEmployeesAsync(employeesFilter);

            var employee = employeeDetails?.Data?.Items?.FirstOrDefault();
            if (employee == null)
            {
                return null;
            }

            return new RegisterEmployeeRequest
            {
                EmployeeNumber = employee.No ?? string.Empty,
                FirstName = employee.FirstName ?? string.Empty,
                MiddleName = employee.MiddleName ?? string.Empty,
                LastName = employee.LastName ?? string.Empty,
                Email = employee.Email ?? string.Empty,
                PhoneNumber = employee.MobilePhoneNo,
                Gender = employee.Gender ?? string.Empty,
                Password = employee.Email ?? string.Empty,
                ConfirmPassword = employee.Email ?? string.Empty


            };
                
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating employee in Business Central: {EmployeeNumber}", employeeNumber);
            throw;
        }
    }

    private async Task<bool> SendRegistrationConfirmationEmailAsync(AppUser user)
    {
        try
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationUrl = $"{_emailSettings.ClientBaseUrl}Auth/ConfirmEmail?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

            var emailBody = $@"
                <h2>Welcome to {_emailSettings.DisplayName}!</h2>
                <p>Hello {user.FirstName},</p>
                <p>Your employee account has been created successfully. Please confirm your email address to complete the setup:</p>
                <p><a href='{confirmationUrl}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Confirm Email Address</a></p>
                <p>If you didn't request this account, please contact HR immediately.</p>
                <p><strong>Note:</strong> This confirmation link will expire in 24 hours.</p>
            ";

            var sendEmailResponse = await _emailService.SendEmailAsync(new SendEmailRequest
            {
                To = user.Email!,
                Subject = "Confirm Your Employee Account",
                Body = emailBody
            });

            if (!sendEmailResponse.Successful)
            {
                _logger.LogError("Failed to send registration confirmation email to: {Email}", user.Email);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send registration confirmation email to: {Email}", user.Email);

            throw;
        }
    }


    private Task<DateTimeOffset?> GetLastPasswordResetRequestAsync(string userId)
    {
        try
        {
            var cacheKey = GetPasswordResetCacheKey(userId);
            var timestamp = _cacheService.Get<DateTimeOffset?>(cacheKey);
            return Task.FromResult(timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read password reset timestamp from cache for user {UserId}", userId);
            throw;
        }
    }

    private Task RecordPasswordResetRequestAsync(string userId)
    {
        try
        {
            var cacheKey = GetPasswordResetCacheKey(userId);
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Keep longer than rate-limit window
            };

            _cacheService.SetAsync(cacheKey, DateTimeOffset.UtcNow, TimeSpan.FromMinutes(10));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to record password reset timestamp for user {UserId}", userId);
            // Don't throw — cache is best-effort
        }

        return Task.CompletedTask;
    }

    

    private static bool IsPasswordStrong(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return false;

        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

        // Require at least 3 of the 4 character types
        int typesCount = (hasUpper ? 1 : 0) + (hasLower ? 1 : 0) + (hasDigit ? 1 : 0) + (hasSpecial ? 1 : 0);

        return typesCount >= 2; // At least 2 different character types
    }

    private async Task<bool> SendPasswordResetConfirmationEmailAsync(AppUser user)
    {
        try
        {
            // Note: We're not generating a new token here - this is just a confirmation email
            var emailBody = $@"
            <h2>Password Reset Confirmation</h2>
            <p>Hello {user.FirstName ?? user.Email},</p>
            <p>Your password has been successfully reset at {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss} UTC.</p>
            <p><strong>Security Notice:</strong> If you did not request this password reset, please contact IT support immediately.</p>
            <p>You can now sign in with your new password.</p>
            <p>For your security, all existing sessions have been terminated.</p>
        ";

            var sendEmailResponse = await _emailService.SendEmailAsync(new SendEmailRequest
            {
                To = user.Email!,
                Subject = "Password Reset Successful - ESS Portal",
                Body = emailBody
            });

            if (!sendEmailResponse.Successful)
            {
                _logger.LogError("Password reset confirmation email failed for {Email}. Success: {Success}, Message: {Message}",
                    user.Email, sendEmailResponse.Successful, sendEmailResponse.Message);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while sending password reset confirmation email to: {Email}", user.Email);
            throw;
        }
    }

    private async Task<bool> HasAuthenticatorConfiguredAsync(AppUser user)
    {
        try
        {
            var totpSecret = await _unitOfWork.UserTotpSecretRepository.GetActiveSecretByUserIdAsync(user.Id);
            return totpSecret != null;
        }
        catch
        {
            throw;
        }
    }

    private async Task CleanupUserSessionAsync(string userId, string reason)
    {
        try
        {
            // Revoke refresh tokens (if using JWT with refresh tokens)
            var sessionCleanUpResponse = await RevokeUserRefreshTokensAsync(userId, reason);
            if (!sessionCleanUpResponse)
            {
                _logger.LogWarning("Failed to revoke refresh tokens for user: {UserId}", userId);
                // Don't fail logout if token revocation fails
            }

            // Clear any cached user data (if using caching)
            await ClearUserCacheAsync(userId);

            try
            {
                await _userManager.Users
                    .Where(u => u.Id == userId)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.UpdatedAt, DateTimeOffset.UtcNow));

            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Error updating user last activity: {UserId}", userId);
                // Don't fail the cleanup if this update fails
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during session cleanup for user: {UserId}", userId);
            // Don't fail logout if cleanup fails
        }
    }

    private async Task<bool> RevokeUserRefreshTokensAsync(string userId, string reason)
    {
        try
        {
            var refreshTokens = await _unitOfWork.TokenRepository.GetActiveTokensByUserIdAsync(userId);
            if (refreshTokens.Any())
            {
                var revokedByIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                await _unitOfWork.TokenRepository.RevokeTokensAsync(refreshTokens, reason, revokedByIp);

                return true;
            }

            return true; // No tokens to revoke is still success
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing refresh token revocation for user: {UserId}", userId);
            throw;
        }
    }

    private Task ClearUserCacheAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Task.CompletedTask;

        try
        {
            // Clear all user-related cache entries
            _cacheService.Remove(GetUserCacheKey(userId));
            _cacheService.Remove(GetUserPermissionsCacheKey(userId));
            _cacheService.Remove(GetPasswordResetCacheKey(userId));

        }
        catch (Exception ex)
        {
            // Log but don't throw — cache clearing is best-effort
            _logger.LogWarning(ex, "Failed to clear cache for user: {UserId}", userId);
        }

        return Task.CompletedTask;
    }

    private static bool IsTotpProvider(string? provider)
    {
        return provider?.ToLowerInvariant() switch
        {
            "authenticator" => true,
            "microsoftauthenticator" => true,
            "totp" => true,
            _ => false
        };
    }

    private async Task MoveTempSecretToPermanentAsync(string userId, TempTotpSecret tempSecret)
    {
        try
        {
            // Create permanent secret
            var permanentSecret = new UserTotpSecret
            {
                Id = Guid.CreateVersion7().ToString(),
                UserId = userId,
                EncryptedSecret = tempSecret.EncryptedSecret, // Same encrypted secret
                IsActive = true,
                LastUsedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId,
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = userId
            };

            await _unitOfWork.UserTotpSecretRepository.CreateAsync(permanentSecret);

            await _unitOfWork.TempTotpSecretRepository.DeleteAsync(tempSecret.Id);

            await _unitOfWork.CompleteAsync();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving temp secret to permanent for user: {UserId}", userId);
            throw;
        }
    }
    
    private async Task<UserSession> EndSessionAsync(string sessionId)
    {
        return await _unitOfWork.SessionRepository.DeleteAsync(sessionId);
    }


    private static string GetPasswordResetCacheKey(string userId) => $"PasswordReset_LastRequest_{userId}";
    private static string GetUserCacheKey(string userId) => $"user_{userId}";
    private static string GetUserPermissionsCacheKey(string userId) => $"user_permissions_{userId}";

    private static string GetEmploymentTypeDescription(Employment_Type? employmentType)
    {
        return employmentType switch
        {
            Employment_Type.Contract => "Contract",
            Employment_Type.Permanent => "Permanent",
            Employment_Type.Trustee => "Trustee",
            Employment_Type.Attachee => "Attachee",
            Employment_Type.Intern => "Intern",
            Employment_Type._blank_ => string.Empty,
            _ => string.Empty
        };
    }

    private static string GetGenderDescription(Gender gender)
    {
        return gender switch
        {
            Gender.Female => "Female",
            Gender.Male => "Male",
            Gender.Intersex => "Intersex",
            Gender._blank_ => string.Empty,
            _ => string.Empty
        };
    }


}
