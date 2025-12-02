using EssPortal.Application.Dtos.ModelFilters;
using EssPortal.Domain.Enums.NavEnums;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Application.Dtos.Auth;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Profile;
using ESSPortal.Application.Extensions;
using ESSPortal.Application.Mappings;
using ESSPortal.Domain.Entities;
using ESSPortal.Domain.Interfaces;
using ESSPortal.Domain.NavEntities;
using ESSPortal.Infrastructure.Configuration;
using ESSPortal.Infrastructure.Utilities;

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

namespace ESSPortal.Infrastructure.Implementations.Services;
internal sealed class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtService _jwtService;
    private readonly IClaimsService _claimsService;
    private readonly ILogger<AuthService> _logger;
    private readonly INavisionService _navisionService;
    private readonly IEmailService _emailService;
    private readonly IFileService _fileService;
    private readonly IEmployeeService _employeeService;
    private readonly EmailSettings _emailSettings;
    private readonly IUnitOfWork _unitOfWork; 
    private readonly ITotpService _totpService; 
    private readonly IEncryptionService _encryptionService;
    private readonly ISessionManagementService _sessionManagementService;
    private readonly ICacheService _cacheService;


    public AuthService(
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

        )
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _httpContextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        _navisionService = navisionService ?? throw new ArgumentNullException(nameof(navisionService));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimsService = claimsService ?? throw new ArgumentNullException(nameof(claimsService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
        _emailSettings = emailSettings.Value ?? throw new ArgumentNullException(nameof(emailSettings));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _totpService = totpService ?? throw new ArgumentNullException(nameof(totpService));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        _sessionManagementService = sessionManagementService ?? throw new ArgumentNullException(nameof(sessionManagementService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));



    }


    public async Task<ApiResponse<bool>> RegisterEmployeeAsync(RegisterEmployeeRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();
        AppUser? createdUser = null;
        bool transactionCompleted = false;

        try
        {
            var employeeData = await ValidateEmployeeInBusinessCentralAsync(request.EmployeeNumber ?? string.Empty);
            if (employeeData == null)
            {
                _logger.LogWarning("Registration attempted with invalid employee number: {EmployeeNumber}", request.EmployeeNumber);
                await _unitOfWork.RollbackTransactionAsync();
                transactionCompleted = true;
                return ApiResponse<bool>.Failure("Invalid employee number. Please contact HR for assistance.");
            }

            var existingUser = await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmployeeNumber == request.EmployeeNumber);

            if (existingUser != null)
            {
                _logger.LogWarning("Duplicate registration attempt for employee: {EmployeeNumber}", request.EmployeeNumber);
                await _unitOfWork.RollbackTransactionAsync();
                transactionCompleted = true;
                return ApiResponse<bool>.Failure("You are already registered. Please log in.");
            }

            var enrichedRequest = new RegisterEmployeeRequest(
                employeeData.EmployeeNumber,
                employeeData.FirstName,
                employeeData.MiddleName,
                employeeData.LastName,
                employeeData.Email,
                employeeData.PhoneNumber,
                employeeData.Gender,
                request.Password,
                request.ConfirmPassword,
                true, // IsActive
                false, // IsDeleted
                request.ReturnUrl,
                request.ExternalLogins ?? []
            );

            var appUser = enrichedRequest.ToAppUser();

            var userCreationResult = await _userManager.CreateAsync(appUser, request.Password ?? string.Empty);
            if (!userCreationResult.Succeeded)
            {
                var errors = string.Join(", ", userCreationResult.Errors.Select(e => e.Description));
                _logger.LogError("User creation failed for employee {EmployeeNumber}: {Errors}", request.EmployeeNumber, errors);
                await _unitOfWork.RollbackTransactionAsync();
                transactionCompleted = true;
                return ApiResponse<bool>.Failure("Account creation failed. Please check your password requirements.");
            }

            createdUser = appUser; // Track for potential cleanup

            var userProfile = new UserProfile
            {
                CreatedBy = appUser.Id,
                UpdatedBy = appUser.Id,
                UserId = appUser.Id,

                // Contact Information - Pre-populate from BC where available
                CountryRegionCode = null,
                PhysicalAddress = null,
                TelephoneNo = appUser.PhoneNumber,
                MobileNo = employeeData.PhoneNumber,
                PostalAddress = null,
                PostCode = null,
                City = null,
                ContactEMailAddress = employeeData.Email,

                // Banking Information - Empty initially
                BankAccountType = null,
                KBABankCode = null,
                KBABranchCode = null,
                BankAccountNo = null
            };

            var createUserProfileResult = await _unitOfWork.UserProfileRepository.CreateAsync(userProfile);
            if (createUserProfileResult == null)
            {
                _logger.LogError("Failed to create user profile for employee {EmployeeNumber}", request.EmployeeNumber);
                throw new InvalidOperationException("Failed to create user profile");
            }

            _logger.LogInformation("Created UserProfile with ID: {ProfileId} for employee {EmployeeNumber}",
                userProfile.Id, request.EmployeeNumber);

            var sendEmailResult = await SendRegistrationConfirmationEmailAsync(appUser);
            if (!sendEmailResult)
            {
                _logger.LogError("Failed to send confirmation email for employee {EmployeeNumber}", request.EmployeeNumber);
                throw new InvalidOperationException("Failed to send confirmation email");
            }

            await _unitOfWork.CompleteAsync(); 
            await _unitOfWork.CommitTransactionAsync();
            transactionCompleted = true;

            _logger.LogInformation("Employee account created successfully: {EmployeeNumber} with UserProfile ID: {ProfileId}",
                request.EmployeeNumber, userProfile.Id);

            return ApiResponse<bool>.Success("Account created successfully! Please check your email to confirm your account.", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for employee {EmployeeNumber}. Rolling back changes.", request.EmployeeNumber);

            try
            {
                if (!transactionCompleted)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogInformation("Database transaction rolled back for employee {EmployeeNumber}", request.EmployeeNumber);
                }

                if (createdUser != null)
                {
                    await _userManager.DeleteAsync(createdUser);
                    _logger.LogInformation("Successfully cleaned up Identity user for employee {EmployeeNumber}", request.EmployeeNumber);
                }

            }
            catch (Exception cleanupEx)
            {
                _logger.LogError(cleanupEx, "Failed to properly rollback changes for employee {EmployeeNumber}", request.EmployeeNumber);

                _logger.LogCritical("Manual cleanup may be required for employee {EmployeeNumber}. " +
                    "Identity User ID: {UserId}", request.EmployeeNumber, createdUser?.Id);
            }

            return ApiResponse<bool>.Failure("Registration failed. Please try again or contact support if the problem persists.");
        }
    }

    public async Task<ApiResponse<bool>> SendEmailConfirmationAsync(SendEmailConfirmationRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal if email exists for security
                _logger.LogWarning("Email confirmation requested for non-existent email: {Email}", request.Email);
                return ApiResponse<bool>.Success("If the email exists, a confirmation link has been sent", true);
            }

            if (user.EmailConfirmed)
            {
                _logger.LogInformation("Email confirmation requested for already confirmed email: {Email}", request.Email);
                return ApiResponse<bool>.Success("Email is already confirmed", true);
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

            _logger.LogInformation("Email confirmation sent to: {Email}", request.Email);
            return ApiResponse<bool>.Success("Confirmation email sent", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email confirmation to: {Email}", request.Email);
            return ApiResponse<bool>.Failure("Unable to send confirmation email");
        }
    }

    public async Task<ApiResponse<bool>> ResendEmailConfirmationAsync(SendEmailConfirmationRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal if email exists for security
                _logger.LogWarning("Confirmation resend requested for non-existent email: {Email}", request.Email);
                return ApiResponse<bool>.Success("If the email exists and is unconfirmed, a new confirmation link has been sent.", true);
            }

            if (user.EmailConfirmed)
            {
                _logger.LogInformation("Confirmation resend requested for already confirmed email: {Email}", request.Email);
                return ApiResponse<bool>.Success("Email is already confirmed. You can sign in now.", true);
            }

            await SendRegistrationConfirmationEmailAsync(user);
            return ApiResponse<bool>.Success("Confirmation email has been resent.", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending confirmation email to: {Email}", request.Email);
            return ApiResponse<bool>.Failure("Unable to resend confirmation email. Please try again later.");
        }
    }

    public async Task<ApiResponse<bool>> ConfirmUserEmailAsync(ConfirmUserEmailRequest confirmEmailRequest)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(confirmEmailRequest.Email);
            if (user == null)
            {
                _logger.LogWarning("Email confirmation attempted for non-existent email: {Email}", confirmEmailRequest.Email);
                return ApiResponse<bool>.Failure("Invalid confirmation link");
            }

            if (user.EmailConfirmed)
            {
                _logger.LogInformation("Email already confirmed for user: {UserId}", user.Id);
                return ApiResponse<bool>.Success("Email is already confirmed", true);
            }

            var result = await _userManager.ConfirmEmailAsync(user, confirmEmailRequest.Token);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Email confirmation failed for user: {UserId}. Errors: {Errors}",
                    user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<bool>.Failure("Invalid or expired confirmation link");
            }

            _logger.LogInformation("Email confirmed successfully for user: {UserId}", user.Id);
            return ApiResponse<bool>.Success("Email confirmed successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email for request: {Email}", confirmEmailRequest.Email);
            return ApiResponse<bool>.Failure("An error occurred during email confirmation");
        }
    }

    public async Task<ApiResponse<LoginResponse>> SignInAsync(LoginRequest loginRequest)
    {
        try
        {
            var user = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(u => u.EmployeeNumber == loginRequest.EmployeeNumber);
            if (user == null)
            {
                _logger.LogWarning("Login attempt with invalid employee number: {EmployeeNumber}", loginRequest.EmployeeNumber);
                return ApiResponse<LoginResponse>.Failure("Invalid Employee Number or password.");
            }

            // Check if email is confirmed
            var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            if (!emailConfirmed)
            {
                _logger.LogWarning("Login attempt with unconfirmed email for user: {UserId}", user.Id);
                return ApiResponse<LoginResponse>.Failure("Please confirm your email before logging in.");
            }

            bool passwordValid = await _userManager.CheckPasswordAsync(user, loginRequest.Password);
            if (!passwordValid)
            {
                _logger.LogWarning("Invalid password attempt for user: {UserId}", user.Id);
                return ApiResponse<LoginResponse>.Failure("Invalid Employee Number or password.");
            }

            // Check if Account is Locked
            var signInResult = await _signInManager.PasswordSignInAsync(user, loginRequest.Password, loginRequest.RememberMe, true);

            if (signInResult.IsLockedOut)
            {
                _logger.LogWarning("Account locked for user: {UserId}", user.Id);
                return ApiResponse<LoginResponse>.Failure("Your account is locked due to multiple failed login attempts. Please reset your password or contact support.");
            }

            if (signInResult.IsNotAllowed)
            {
                _logger.LogWarning("Sign in not allowed for user: {UserId}", user.Id);
                return ApiResponse<LoginResponse>.Failure("Sign in not allowed. Please contact support.");
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
                false,
                twoFactorEnabled,
                user.LastLoginAt,
                []
            );

            //var userInfo = new UserInfo(
            //    user.Id,
            //    string.Empty,
            //    user.EmployeeNumber,
            //    user.FirstName,
            //    user.LastName,
            //    user.Email,
            //    user.PhoneNumber,
            //    string.Empty,
            //    string.Empty,
            //    string.Empty,
            //    string.Empty,
            //    user.ProfilePictureUrl,
            //    string.Empty,
            //    emailConfirmed,
            //    false, //phoneConfirmed,
            //    twoFactorEnabled,
            //    user.LastLoginAt,
            //    []  // Will be populated below
            //);

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
                    return ApiResponse<LoginResponse>.Failure("Could not generate temporary authentication token");
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userInfoWith2FA = userInfo with { Roles = roles.ToList() };

                return ApiResponse<LoginResponse>.Success("Two-factor authentication required", new LoginResponse(
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
                return ApiResponse<LoginResponse>.Failure("Invalid login attempt.");
            }

            var sessionId = Guid.NewGuid().ToString();

            var sessionCreationResult = await _sessionManagementService.CreateSessionAsync(
                user.Id,
                sessionId,
                _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown",
                _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "unknown",
                loginRequest.DeviceFingerprint ?? "unknown"
            );

            if (!sessionCreationResult.Successful)
            {
                _logger.LogError("Failed to create user session for user {UserId}", user.Id);
                return ApiResponse<LoginResponse>.Failure("Could not establish a user session.");
            }

            var userClaims = await _claimsService.GetUserClaimsAsync(user);
            if (!userClaims.Successful || userClaims.Data == null || userClaims.Data.Count == 0)
            {
                _logger.LogError("Failed to get user claims for user: {UserId}", user.Id);
                return ApiResponse<LoginResponse>.Failure("Could not retrieve user claims");
            }

            var tokenResponse = await _jwtService.GenerateToken(user);
            if (!tokenResponse.Successful || string.IsNullOrWhiteSpace(tokenResponse.Data))
            {
                _logger.LogError("Failed to generate token for user: {UserId}", user.Id);
                return ApiResponse<LoginResponse>.Failure("Could not generate authentication token");
            }

            var refreshToken = _jwtService.GenerateRefreshToken(user);
            if (!refreshToken.Successful || string.IsNullOrWhiteSpace(refreshToken.Data))
            {
                _logger.LogError("Failed to generate refresh token for user: {UserId}", user.Id);
                return ApiResponse<LoginResponse>.Failure("Could not generate refresh token");
            }

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwt = jwtHandler.ReadJwtToken(tokenResponse.Data);
            var tokenExpiry = jwt.ValidTo;

            var tokenValidationResponse = _jwtService.IsTokenValid(tokenResponse.Data);
            if (!tokenValidationResponse.Successful || !tokenValidationResponse.Data)
            {
                _logger.LogError("Token validation failed for user: {UserId}", user.Id);
                return ApiResponse<LoginResponse>.Failure("Invalid authentication token");
            }

            var isSecurityTokenValid = tokenValidationResponse.Data;

            if (!isSecurityTokenValid)
            {
                throw new SecurityTokenValidationException($"Error|Token is Invalid");
            }

            var rolesResponse = await _userManager.GetRolesAsync(user);
            var userRoles = rolesResponse.ToList();
            var finalUserInfo = userInfo with { Roles = userRoles };

            await _userManager.Users
                .Where(u => u.Id == user.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.LastLoginAt, DateTimeOffset.UtcNow));

            return ApiResponse<LoginResponse>.Success("Login successful", new LoginResponse(
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
            );

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for employee number: {EmployeeNumber}", loginRequest.EmployeeNumber);
            throw;
        }
    }

    public async Task<ApiResponse<CurrentUserResponse>> GetCurrentUserAsync()
    {
        try
        {
            var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(userName))
                return ApiResponse<CurrentUserResponse>.Failure("User not authenticated");

            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return ApiResponse<CurrentUserResponse>.Failure("User not found.");

            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null)
                return ApiResponse<CurrentUserResponse>.Failure("User not found.");

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

            return ApiResponse<CurrentUserResponse>.Success("CurrentUser", new CurrentUserResponse(
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

    public async Task<ApiResponse<ProviderResponse>> Get2FAProvidersAsync(Get2FAProviderRequest providerRequest)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(providerRequest.UserId);
            if (user == null)
                return ApiResponse<ProviderResponse>.Failure("User not found.");

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

            return ApiResponse<ProviderResponse>.Success("Providers retrieved", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting 2FA providers for user: {UserId}", providerRequest.UserId);
            return ApiResponse<ProviderResponse>.Failure("Failed to retrieve 2FA providers.");
        }
    }

    public async Task<ApiResponse<Send2FACodeResponse>> Send2FACodeAsync(Send2FACodeRequest sendCodeRequest)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(sendCodeRequest.UserId);
            if (user == null)
            {
                _logger.LogWarning("2FA code send attempt for non-existent user: {UserId}", sendCodeRequest.UserId);
                return ApiResponse<Send2FACodeResponse>.Failure("User not found");
            }

            var validProviders = await _userManager.GetValidTwoFactorProvidersAsync(user);
            if (!validProviders.Contains(sendCodeRequest.SelectedProvider))
                return ApiResponse<Send2FACodeResponse>.Failure("Invalid provider. Choose 'Email' or 'Phone'.");

            var token = await _userManager.GenerateTwoFactorTokenAsync(user, sendCodeRequest.SelectedProvider);
            if (string.IsNullOrWhiteSpace(token))
                return ApiResponse<Send2FACodeResponse>.Failure("Failed to generate authentication token.");

            if (sendCodeRequest.SelectedProvider == "Email")
            {
                await _emailService.SendEmailAsync(new SendEmailRequest
                {
                    To = user.Email,
                    Subject = "Your Two-Factor Authentication Code",
                    Body = $"Your 2FA code is: {token}"
                });
            }

            _logger.LogInformation("2FA code sent to user {UserId} via {Provider}", sendCodeRequest.UserId, sendCodeRequest.SelectedProvider);

            return ApiResponse<Send2FACodeResponse>.Success("2FA code sent successfully.",
                new Send2FACodeResponse(
                    user.Id,
                    sendCodeRequest.SelectedProvider,
                    sendCodeRequest.SelectedProvider ?? validProviders.FirstOrDefault()!,
                    token, // ToDo: Consider encoding the token for security or Remove it from the response
                    DateTimeOffset.UtcNow,
                    DateTimeOffset.UtcNow.AddMinutes(5), 
                    true,
                    TimeSpan.FromSeconds(30), 
                    string.Empty
                    ));

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending 2FA code for user: {UserId}", sendCodeRequest.UserId);
            throw;
        }
    }

    public async Task<ApiResponse<Verify2FACodeResponse>> Verify2FACodeAsync(Verify2FACodeRequest verifyCodeRequest)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(verifyCodeRequest.UserId);
            if (user == null)
            {
                _logger.LogWarning("2FA verification attempt for non-existent user: {UserId}", verifyCodeRequest.UserId);
                return ApiResponse<Verify2FACodeResponse>.Failure("User not found");
            }

            bool isValidCode = false;
            string decryptedSecret = string.Empty;

            if (IsTotpProvider(verifyCodeRequest.Provider))
            {
                var tempSecret = await _unitOfWork.TempTotpSecretRepository.GetValidTempSecretByUserIdAsync(user.Id);

                if (tempSecret != null)
                {
                    // This is during setup - use temp secret
                    _logger.LogInformation("Using temp secret for 2FA verification during setup for user: {UserId}", user.Id);

                    decryptedSecret = _encryptionService.Decrypt(tempSecret.EncryptedSecret);

                    isValidCode = _totpService.VerifyTotpCode(decryptedSecret, verifyCodeRequest.Code);

                    if (isValidCode)
                    {
                        await MoveTempSecretToPermanentAsync(user.Id, tempSecret);

                        await _userManager.SetTwoFactorEnabledAsync(user, true);

                        _logger.LogInformation("2FA enabled for user during setup: {UserId}", user.Id);
                    }
                }
                else
                {
                    var totpSecret = await _unitOfWork.UserTotpSecretRepository.GetActiveSecretByUserIdAsync(user.Id);

                    if (totpSecret == null)
                    {
                        _logger.LogWarning("No TOTP secret found for user: {UserId}", user.Id);
                        return ApiResponse<Verify2FACodeResponse>.Failure("Authenticator app not configured. Please set it up first.");
                    }

                    decryptedSecret = _encryptionService.Decrypt(totpSecret.EncryptedSecret);

                    isValidCode = _totpService.VerifyTotpCode(decryptedSecret, verifyCodeRequest.Code);
                }

                if (!isValidCode)
                {
                    _logger.LogWarning("Invalid TOTP code for user: {UserId}. Code: {Code}", verifyCodeRequest.UserId, verifyCodeRequest.Code);

                    return ApiResponse<Verify2FACodeResponse>.Failure("Invalid verification code. Please try again.");
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
                    _logger.LogWarning("Invalid 2FA code for user: {UserId} via {Provider}",
                        verifyCodeRequest.UserId, verifyCodeRequest.Provider);
                    return ApiResponse<Verify2FACodeResponse>.Failure("Invalid verification code");
                }
            }

            // Rest of the method remains the same...
            var userClaims = await _claimsService.GetUserClaimsAsync(user);
            if (!userClaims.Successful || userClaims.Data == null)
            {
                _logger.LogError("Failed to get user claims for user: {UserId}", user.Id);
                return ApiResponse<Verify2FACodeResponse>.Failure("Could not retrieve user claims");
            }

            var tokenResponse = await _jwtService.GenerateToken(user);
            if (!tokenResponse.Successful || string.IsNullOrWhiteSpace(tokenResponse.Data))
            {
                _logger.LogError("Failed to generate token for user: {UserId}", user.Id);
                return ApiResponse<Verify2FACodeResponse>.Failure("Could not generate authentication token");
            }

            var tokenExpiry = _jwtService.GetTokenExpiry(tokenResponse.Data);
            var refreshToken = _jwtService.GenerateRefreshToken(user);

            // Continue with the rest of your existing code...

            _logger.LogInformation("User {UserId} completed 2FA verification successfully via {Provider}",
                verifyCodeRequest.UserId, verifyCodeRequest.Provider);

            // Update last login and sign in user
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

            return ApiResponse<Verify2FACodeResponse>.Success("Two-factor authentication successful",
                new Verify2FACodeResponse(
                    tokenResponse.Data,
                    refreshToken.Data ?? string.Empty,
                    user.Id,
                    true,
                    tokenExpiry.Data,
                    userInfo, // Build this as in your existing code
                    claimsResponse // Build this as in your existing code
                ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during 2FA verification for user: {UserId}", verifyCodeRequest.UserId);
            return ApiResponse<Verify2FACodeResponse>.Failure("An error occurred during verification");
        }
    }

    public async Task<ApiResponse<bool>> RequestPasswordResetAsync(ForgotPasswordRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            // Always return success to prevent email enumeration attacks
            if (user == null || !user.EmailConfirmed)
            {
                _logger.LogWarning("Password reset requested for non-existent or unconfirmed email: {Email}", request.Email);
                return ApiResponse<bool>.Success("If the email exists, a reset link has been sent", true);
            }

            if (!user.IsActive || user.IsDeleted)
            {
                _logger.LogWarning("Password reset requested for inactive user: {UserId}", user.Id);
                return ApiResponse<bool>.Success("If the email exists, a reset link has been sent", true);
            }

            // Check for rate limiting - prevent spam requests
            var lastResetRequest = await GetLastPasswordResetRequestAsync(user.Id);
            if (lastResetRequest.HasValue && lastResetRequest.Value.AddMinutes(5) > DateTimeOffset.UtcNow)
            {
                _logger.LogWarning("Password reset rate limit exceeded for user: {UserId}", user.Id);
                return ApiResponse<bool>.Success("If the email exists, a reset link has been sent", true);
            }

            await RecordPasswordResetRequestAsync(user.Id);

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            if (string.IsNullOrWhiteSpace(resetToken))
            {
                _logger.LogError("Failed to generate password reset token for user: {UserId}", user.Id);
                return ApiResponse<bool>.Success("If the email exists, a reset link has been sent", true);
            }

            var resetUrl = $"{_emailSettings.ClientBaseUrl}Auth/ResetPassword?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(resetToken)}";

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

            _logger.LogInformation("Password reset email sent to: {Email}", user.Email);
            return ApiResponse<bool>.Success("If the email exists, a reset link has been sent", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting password reset for: {Email}", request.Email);
            return ApiResponse<bool>.Failure("Unable to process password reset request");
        }
    }

    public async Task<ApiResponse<bool>> ValidatePasswordResetTokenAsync(ValidateResetTokenRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Token))
            {
                return ApiResponse<bool>.Failure("Invalid reset request.");
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Password reset validation attempted for non-existent email: {Email}", request.Email);
                return ApiResponse<bool>.Failure("Invalid or expired reset link");
            }

            var isValidToken = await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword",request.Token);
            
            if (!isValidToken)
            {
                _logger.LogWarning("Invalid password reset token used for user: {UserId}", user.Id);
                return ApiResponse<bool>.Failure("Invalid or expired reset link");
            }

            return ApiResponse<bool>.Success("Reset link is valid", true);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating password reset token for: {Email}", request.Email);
            return ApiResponse<bool>.Failure("Unable to validate reset link");
            throw;
        }
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();
        AppUser? originalUser = null;
        bool transactionCompleted = false;

        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Password reset attempted for non-existent email: {Email}", request.Email);
                await _unitOfWork.RollbackTransactionAsync();
                transactionCompleted = true;
                return ApiResponse<bool>.Failure("Invalid reset request");
            }

            // Store original user state for potential rollback
            originalUser = new AppUser
            {
                Id = user.Id,
                PasswordHash = user.PasswordHash,
                SecurityStamp = user.SecurityStamp,
                FailedLoginAttempts = user.FailedLoginAttempts,
                PasswordLastChanged = user.PasswordLastChanged,
                RequirePasswordChange = user.RequirePasswordChange,
                UpdatedAt = user.UpdatedAt
            };

            var isValidToken = await _userManager.VerifyUserTokenAsync(
                user,
                _userManager.Options.Tokens.PasswordResetTokenProvider,
                "ResetPassword",
                request.Token
            );

            if (!isValidToken)
            {
                _logger.LogWarning("Invalid password reset token used for user: {UserId}", user.Id);
                await _unitOfWork.RollbackTransactionAsync();
                transactionCompleted = true;
                return ApiResponse<bool>.Failure("Invalid or expired reset link");
            }

            var password = request.Password ?? request.NewPassword ?? string.Empty;

            // Additional password validation (beyond data annotations)
            if (!IsPasswordStrong(password))
            {
                await _unitOfWork.RollbackTransactionAsync();
                transactionCompleted = true;
                return ApiResponse<bool>.Failure("Password does not meet security requirements");
            }

            // Check if new password is different from current password
            var isSamePassword = await _userManager.CheckPasswordAsync(user, password);
            if (isSamePassword)
            {
                await _unitOfWork.RollbackTransactionAsync();
                transactionCompleted = true;
                return ApiResponse<bool>.Failure("New password must be different from your current password");
            }

            // Reset password using Identity (this is outside EF transaction)
            var resetResult = await _userManager.ResetPasswordAsync(user, request.Token, password);
            if (!resetResult.Succeeded)
            {
                var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Password reset failed for user {UserId}: {Errors}", user.Id, errors);
                await _unitOfWork.RollbackTransactionAsync();
                transactionCompleted = true;
                return ApiResponse<bool>.Failure("Password reset failed. Please ensure your password meets all requirements.");
            }

            // Update user security info
            user.ResetFailedLoginAttempts();
            user.PasswordLastChanged = DateTimeOffset.UtcNow;
            user.RequirePasswordChange = false;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                _logger.LogError("Failed to update user security info for user: {UserId}", user.Id);
                throw new InvalidOperationException("Failed to update user security information");
            }

            // Revoke all existing refresh tokens (within transaction)
            var refreshTokens = await _unitOfWork.TokenRepository.GetActiveTokensByUserIdAsync(user.Id);
            if (refreshTokens.Any())
            {
                var revokedByIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                await _unitOfWork.TokenRepository.RevokeTokensAsync(refreshTokens, "Password reset", revokedByIp);
                _logger.LogInformation("Revoked {Count} refresh tokens for user: {UserId}",
                    refreshTokens.Count(), user.Id);
            }

            // Send confirmation email BEFORE committing transaction
            var sendEmailResponse = await SendPasswordResetConfirmationEmailAsync(user);
            if (!sendEmailResponse)
            {
                _logger.LogError("Failed to send password reset confirmation email for user: {UserId}", user.Id);
                throw new InvalidOperationException("Failed to send confirmation email");
            }

            // Save all EF changes and commit transaction
            await _unitOfWork.CompleteAsync();
            await _unitOfWork.CommitTransactionAsync();
            transactionCompleted = true;

            // Clear any cached data for this user (outside transaction)
            await ClearUserCacheAsync(user.Id);

            _logger.LogInformation("Password reset successful for user: {UserId}", user.Id);
            return ApiResponse<bool>.Success("Password reset successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password reset failed for email: {Email}. Rolling back changes.", request.Email);

            try
            {
                // Rollback database transaction
                if (!transactionCompleted)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogInformation("Database transaction rolled back for password reset: {Email}", request.Email);
                }

                // Rollback Identity changes if user was modified
                if (originalUser != null)
                {
                    var currentUser = await _userManager.FindByIdAsync(originalUser.Id);
                    if (currentUser != null)
                    {
                        // Restore original password and security info
                        currentUser.PasswordHash = originalUser.PasswordHash;
                        currentUser.SecurityStamp = originalUser.SecurityStamp;
                        currentUser.FailedLoginAttempts = originalUser.FailedLoginAttempts;
                        currentUser.PasswordLastChanged = originalUser.PasswordLastChanged;
                        currentUser.RequirePasswordChange = originalUser.RequirePasswordChange;
                        currentUser.UpdatedAt = originalUser.UpdatedAt;

                        await _userManager.UpdateAsync(currentUser);
                        _logger.LogInformation("Successfully rolled back Identity user changes for: {UserId}", originalUser.Id);
                    }
                }
            }
            catch (Exception cleanupEx)
            {
                _logger.LogError(cleanupEx, "Failed to properly rollback password reset changes for email: {Email}", request.Email);
                _logger.LogCritical("Manual intervention may be required for user password reset rollback: {Email}", request.Email);
            }

            return ApiResponse<bool>.Failure("Password reset failed. Please try again or contact support if the problem persists.");
        }
    }

    public async Task<ApiResponse<bool>> VerifyPasswordAsync(VerifyPasswordRequest verifyPasswordRequest)
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
                return ApiResponse<bool>.Failure("User not found");
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return ApiResponse<bool>.Failure("Account is locked");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, verifyPasswordRequest.Password);
            if (!isPasswordValid)
            {
                // Increment failed access count
                await _userManager.AccessFailedAsync(user);

                // Check if now locked
                if (await _userManager.IsLockedOutAsync(user))
                {
                    return ApiResponse<bool>.Failure("Account locked due to too many failed attempts");
                }

                return ApiResponse<bool>.Failure("Invalid password");
            }

            // Reset failed access count on successful verification
            await _userManager.ResetAccessFailedCountAsync(user);

            return ApiResponse<bool>.Success("Password verified successfully", true);

        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<ApiResponse<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            // Validate the access token format (don't check expiry, it might be expired)
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null || principal.Data == null)
            {
                _logger.LogWarning("Invalid access token format provided for refresh");
                return ApiResponse<RefreshTokenResponse>.Failure("Invalid access token format");
            }

            // Get user ID from the token
            var userId = principal.Data.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("No user ID found in access token");
                return ApiResponse<RefreshTokenResponse>.Failure("Invalid token: No user ID found");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for refresh token: {UserId}", userId);
                return ApiResponse<RefreshTokenResponse>.Failure("User not found");
            }

            if (!user.IsActive || user.IsDeleted)
            {
                _logger.LogWarning("Refresh token attempt for inactive user: {UserId}", userId);
                return ApiResponse<RefreshTokenResponse>.Failure("User account is not active");
            }

            // Validate the refresh token
            var storedRefreshToken = await _unitOfWork.TokenRepository.GetRefreshTokenAsync(request.RefreshToken, userId);
            if (storedRefreshToken == null)
            {
                _logger.LogWarning("Refresh token not found for user: {UserId}", userId);
                return ApiResponse<RefreshTokenResponse>.Failure("Invalid refresh token");
            }

            if (storedRefreshToken.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                _logger.LogWarning("Expired refresh token used for user: {UserId}", userId);
                await _unitOfWork.TokenRepository.RevokeRefreshTokenAsync(storedRefreshToken, "Token expired");
                return ApiResponse<RefreshTokenResponse>.Failure("Refresh token has expired");
            }

            if (storedRefreshToken.IsRevoked)
            {
                _logger.LogWarning("Revoked refresh token used for user: {UserId}", userId);
                return ApiResponse<RefreshTokenResponse>.Failure("Refresh token has been revoked");
            }

            // Check if refresh token is used (prevent reuse)
            if (storedRefreshToken.IsUsed)
            {
                _logger.LogWarning("Already used refresh token attempted for user: {UserId}", userId);
                // Revoke all tokens for this user for security
                await _unitOfWork.TokenRepository.RevokeAllUserTokensAsync(userId, "Token reuse detected");
                return ApiResponse<RefreshTokenResponse>.Failure("Refresh token has already been used");
            }

            // Validate that the refresh token belongs to the same user as the access token
            if (storedRefreshToken.UserId != userId)
            {
                _logger.LogWarning("Refresh token user mismatch. Token UserId: {TokenUserId}, Request UserId: {RequestUserId}",
                    storedRefreshToken.UserId, userId);
                return ApiResponse<RefreshTokenResponse>.Failure("Token mismatch");
            }

            // Generate new tokens
            var userClaims = await _claimsService.GetUserClaimsAsync(user);
            if (!userClaims.Successful || userClaims.Data == null)
            {
                _logger.LogError("Failed to get user claims during token refresh for user: {UserId}", userId);
                return ApiResponse<RefreshTokenResponse>.Failure("Could not retrieve user claims");
            }

            var newAccessTokenResponse = await _jwtService.GenerateToken(user);
            if (!newAccessTokenResponse.Successful || string.IsNullOrWhiteSpace(newAccessTokenResponse.Data))
            {
                _logger.LogError("Failed to generate new access token for user: {UserId}", userId);
                return ApiResponse<RefreshTokenResponse>.Failure("Could not generate new access token");
            }

            var newRefreshTokenResponse = _jwtService.GenerateRefreshToken(user);
            if (!newRefreshTokenResponse.Successful || string.IsNullOrWhiteSpace(newRefreshTokenResponse.Data))
            {
                _logger.LogError("Failed to generate new refresh token for user: {UserId}", userId);
                return ApiResponse<RefreshTokenResponse>.Failure("Could not generate new refresh token");
            }

            var tokenExpiry = _jwtService.GetTokenExpiry(newAccessTokenResponse.Data);
            if (!tokenExpiry.Successful)
            {
                _logger.LogError("Failed to get token expiry for user: {UserId}", userId);
                return ApiResponse<RefreshTokenResponse>.Failure("Could not determine token expiry");
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

            _logger.LogInformation("Token refreshed successfully for user: {UserId}", userId);

            return ApiResponse<RefreshTokenResponse>.Success("Token refreshed successfully", new RefreshTokenResponse(
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

    public async Task<ApiResponse<bool>> SignOutAsync()
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

            return ApiResponse<bool>.Success("Signed out successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during API sign out");
            return ApiResponse<bool>.Failure("Sign out failed");
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
                _logger.LogWarning("Could not retrieve employee details for: {EmployeeNumber}", employeeNumber);
                return null;
            }

            return new RegisterEmployeeRequest(
                employee.No, 
                employee.First_Name, 
                employee.Middle_Name, 
                employee.Last_Name,
                employee.E_Mail, 
                employee.Mobile_Phone_No,
                employee.Gender == "Male" ? Gender.Male : Gender.Female,
                employee.E_Mail, 
                employee.E_Mail,
                false,
                false, 
                string.Empty, 
                []);
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating employee in Business Central: {EmployeeNumber}", employeeNumber);
            return null;
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

            _logger.LogInformation("Registration confirmation email sent to: {Email}", user.Email);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send registration confirmation email to: {Email}", user.Email);

            return false;
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
            return Task.FromResult<DateTimeOffset?>(null); // Fail open
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

            _cacheService.Set(cacheKey, DateTimeOffset.UtcNow, options);
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

            _logger.LogInformation("Password reset confirmation email sent successfully to: {Email}", user.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while sending password reset confirmation email to: {Email}", user.Email);
            return false;
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
            return false;
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

                _logger.LogDebug("Updated last activity for user: {UserId}", userId);
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

                _logger.LogInformation("Marked {Count} refresh tokens for revocation for user: {UserId}", refreshTokens.Count(), userId);

                return true;
            }

            return true; // No tokens to revoke is still success
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing refresh token revocation for user: {UserId}", userId);
            return false;
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

            _logger.LogDebug("Cleared cache for user: {UserId}", userId);
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
                Id = Guid.NewGuid().ToString(),
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

            _logger.LogInformation("Moved temp TOTP secret to permanent for user: {UserId}", userId);
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

    
}
