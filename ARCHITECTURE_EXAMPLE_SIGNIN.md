# Clean Architecture Example: Sign-In Process

This document shows the **correct way** to implement the sign-in feature using Clean Architecture principles.

---

## Current Problem

**What you have now**:
1. User enters credentials in MVC form
2. MVC AuthController calls `ApiService.PostAsync()`
3. HTTP request sent to API project
4. API AuthController receives request
5. API calls `AuthService.LoginAsync()`
6. AuthService validates credentials
7. Response serialized to JSON
8. Sent back over HTTP
9. MVC deserializes JSON
10. MVC creates cookie and signs in user

**Problems**:
- Network overhead for local operation
- Serialization/deserialization overhead
- Two authentication systems (JWT for API, Cookies for MVC)
- Duplicate AuthService in both Infrastructure and UI
- Error handling complexity
- Can't share transactions

---

## Correct Architecture

**How it should work**:
1. User enters credentials in MVC/API
2. Controller creates a Command
3. Command sent to Handler
4. Handler orchestrates domain logic
5. Response returned directly
6. Controller handles response

**Benefits**:
- In-memory call (microseconds vs milliseconds)
- Single source of truth
- Shared business logic
- Clean separation of concerns
- Easy to test
- Easy to extend

---

## Layer-by-Layer Implementation

### 1. Domain Layer (Core Business)

```csharp
// src/Domain/ESSPortal.Domain/Entities/Core/Employee.cs
namespace ESSPortal.Domain.Entities.Core;

public class Employee : BaseEntity
{
    // Value Objects
    public EmployeeNumber EmployeeNumber { get; private set; }
    public EmailAddress Email { get; private set; }
    public FullName Name { get; private set; }

    // Properties
    public string PasswordHash { get; private set; }
    public bool IsActive { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public bool TwoFactorEnabled { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEnd { get; private set; }

    // Private constructor (enforce creation through factory methods)
    private Employee() { }

    // Factory method for registration
    public static Employee Register(
        EmployeeNumber employeeNumber,
        EmailAddress email,
        FullName name,
        string passwordHash)
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeNumber = employeeNumber,
            Email = email,
            Name = name,
            PasswordHash = passwordHash,
            IsActive = true,
            EmailConfirmed = false,
            TwoFactorEnabled = false,
            FailedLoginAttempts = 0,
            CreatedAt = DateTime.UtcNow
        };

        // Domain event
        employee.AddDomainEvent(new EmployeeRegisteredEvent(employee.Id, email.Value));

        return employee;
    }

    // Business logic methods
    public Result RecordSuccessfulLogin()
    {
        if (!IsActive)
            return Result.Failure("Account is not active");

        if (IsLockedOut())
            return Result.Failure("Account is locked");

        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;

        AddDomainEvent(new EmployeeLoggedInEvent(Id, Email.Value));

        return Result.Success();
    }

    public Result RecordFailedLogin()
    {
        FailedLoginAttempts++;

        // Lock account after 5 failed attempts
        if (FailedLoginAttempts >= 5)
        {
            LockoutEnd = DateTime.UtcNow.AddMinutes(30);
            AddDomainEvent(new EmployeeLockedOutEvent(Id, Email.Value));
        }

        return Result.Success();
    }

    public bool IsLockedOut()
    {
        return LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
    }

    public Result ValidatePassword(string passwordHash)
    {
        if (PasswordHash != passwordHash)
            return Result.Failure("Invalid password");

        return Result.Success();
    }
}


// src/Domain/ESSPortal.Domain/ValueObjects/EmployeeNumber.cs
public record EmployeeNumber
{
    public string Value { get; }

    public EmployeeNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Employee number cannot be empty");

        if (value.Length > 20)
            throw new ArgumentException("Employee number too long");

        Value = value.Trim().ToUpperInvariant();
    }

    public static implicit operator string(EmployeeNumber empNum) => empNum.Value;
}


// src/Domain/ESSPortal.Domain/ValueObjects/EmailAddress.cs
public record EmailAddress
{
    public string Value { get; }

    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty");

        if (!IsValidEmail(value))
            throw new ArgumentException("Invalid email format");

        Value = value.Trim().ToLowerInvariant();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static implicit operator string(EmailAddress email) => email.Value;
}


// src/Domain/ESSPortal.Domain/IRepositories/IEmployeeRepository.cs
public interface IEmployeeRepository : IRepository<Employee>
{
    Task<Employee?> GetByEmployeeNumberAsync(EmployeeNumber employeeNumber);
    Task<Employee?> GetByEmailAsync(EmailAddress email);
    Task<bool> ExistsAsync(EmployeeNumber employeeNumber);
}


// src/Domain/ESSPortal.Domain/Common/Result.cs
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && error != string.Empty)
            throw new InvalidOperationException();
        if (!isSuccess && error == string.Empty)
            throw new InvalidOperationException();

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, string.Empty);
    public static Result<T> Failure<T>(string error) => new(default!, false, error);
}

public class Result<T> : Result
{
    public T Value { get; }

    protected internal Result(T value, bool isSuccess, string error)
        : base(isSuccess, error)
    {
        Value = value;
    }
}
```

---

### 2. Application Layer (Use Cases)

```csharp
// src/Application/ESSPortal.Application/Commands/Auth/LoginCommand.cs
namespace ESSPortal.Application.Commands.Auth;

public record LoginCommand(
    string EmailOrEmployeeNumber,
    string Password,
    bool RememberMe
);


// src/Application/ESSPortal.Application/Commands/Auth/LoginCommandHandler.cs
public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResult>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ISessionManager _sessionManager;
    private readonly ILogger<LoginCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IEmployeeRepository employeeRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        ISessionManager sessionManager,
        ILogger<LoginCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _sessionManager = sessionManager;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginResult>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        // 1. Find employee (by email or employee number)
        var employee = await FindEmployeeAsync(command.EmailOrEmployeeNumber);

        if (employee == null)
        {
            _logger.LogWarning("Login attempt with invalid identifier: {Identifier}",
                command.EmailOrEmployeeNumber);
            return Result.Failure<LoginResult>("Invalid credentials");
        }

        // 2. Check if locked out
        if (employee.IsLockedOut())
        {
            _logger.LogWarning("Login attempt for locked out account: {EmployeeNumber}",
                employee.EmployeeNumber);
            return Result.Failure<LoginResult>(
                $"Account is locked until {employee.LockoutEnd?.ToString("HH:mm")}");
        }

        // 3. Verify password
        var passwordHash = _passwordHasher.HashPassword(command.Password);
        var passwordResult = employee.ValidatePassword(passwordHash);

        if (passwordResult.IsFailure)
        {
            // Record failed attempt
            employee.RecordFailedLogin();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("Failed login attempt for: {EmployeeNumber}",
                employee.EmployeeNumber);

            return Result.Failure<LoginResult>("Invalid credentials");
        }

        // 4. Check if 2FA is enabled
        if (employee.TwoFactorEnabled)
        {
            // Generate and send 2FA code
            var twoFactorToken = await _sessionManager.CreateTwoFactorSessionAsync(employee.Id);

            return Result.Success(new LoginResult
            {
                Requires2FA = true,
                TwoFactorToken = twoFactorToken,
                EmployeeId = employee.Id
            });
        }

        // 5. Record successful login
        var loginResult = employee.RecordSuccessfulLogin();
        if (loginResult.IsFailure)
            return Result.Failure<LoginResult>(loginResult.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Generate tokens
        var accessToken = _tokenGenerator.GenerateAccessToken(employee);
        var refreshToken = _tokenGenerator.GenerateRefreshToken();

        // 7. Create session
        await _sessionManager.CreateSessionAsync(
            employee.Id,
            refreshToken,
            command.RememberMe ? TimeSpan.FromDays(30) : TimeSpan.FromHours(8));

        _logger.LogInformation("Successful login for: {EmployeeNumber}",
            employee.EmployeeNumber);

        return Result.Success(new LoginResult
        {
            Requires2FA = false,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Employee = new EmployeeDto
            {
                Id = employee.Id,
                EmployeeNumber = employee.EmployeeNumber,
                Email = employee.Email,
                FullName = employee.Name.ToString()
            }
        });
    }

    private async Task<Employee?> FindEmployeeAsync(string identifier)
    {
        // Try as email first
        if (identifier.Contains('@'))
        {
            return await _employeeRepository.GetByEmailAsync(new EmailAddress(identifier));
        }

        // Try as employee number
        return await _employeeRepository.GetByEmployeeNumberAsync(new EmployeeNumber(identifier));
    }
}


// src/Application/ESSPortal.Application/DTOs/Auth/LoginResult.cs
public class LoginResult
{
    public bool Requires2FA { get; init; }
    public string? TwoFactorToken { get; init; }
    public Guid? EmployeeId { get; init; }

    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public EmployeeDto? Employee { get; init; }
}


// src/Application/ESSPortal.Application/Interfaces/ICommandHandler.cs
public interface ICommandHandler<in TCommand, TResult>
{
    Task<Result<TResult>> Handle(TCommand command, CancellationToken cancellationToken = default);
}
```

---

### 3. Infrastructure Layer (Technical Implementations)

```csharp
// src/Infrastructure/ESSPortal.Infrastructure/Persistence/Repositories/EmployeeRepository.cs
namespace ESSPortal.Infrastructure.Persistence.Repositories;

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Employee?> GetByEmployeeNumberAsync(EmployeeNumber employeeNumber)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeNumber == employeeNumber);
    }

    public async Task<Employee?> GetByEmailAsync(EmailAddress email)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.Email == email);
    }

    public async Task<bool> ExistsAsync(EmployeeNumber employeeNumber)
    {
        return await _context.Employees
            .AnyAsync(e => e.EmployeeNumber == employeeNumber);
    }
}


// src/Infrastructure/ESSPortal.Infrastructure/Security/PasswordHasher.cs
public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public bool VerifyPassword(string password, string hash)
    {
        var computedHash = HashPassword(password);
        return computedHash == hash;
    }
}


// src/Infrastructure/ESSPortal.Infrastructure/Security/JwtTokenGenerator.cs
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateAccessToken(Employee employee)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new Claim(ClaimTypes.Email, employee.Email),
            new Claim("employee_number", employee.EmployeeNumber),
            new Claim(ClaimTypes.Name, employee.Name.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}


// src/Infrastructure/ESSPortal.Infrastructure.NAV/Adapters/NavEmployeeAdapter.cs
// (This is how you integrate with NAV - as an adapter, not in Domain!)
namespace ESSPortal.Infrastructure.NAV.Adapters;

public class NavEmployeeAdapter : INavEmployeeAdapter
{
    private readonly INavODataClient _navClient;
    private readonly ILogger<NavEmployeeAdapter> _logger;

    public async Task<Employee?> GetEmployeeFromNavAsync(EmployeeNumber employeeNumber)
    {
        try
        {
            // Get NAV model
            var navEmployee = await _navClient.GetEmployeeCardAsync(employeeNumber.Value);

            if (navEmployee == null)
                return null;

            // Map NAV model → Domain model
            return Employee.Register(
                new EmployeeNumber(navEmployee.No),
                new EmailAddress(navEmployee.Company_E_Mail),
                new FullName(navEmployee.First_Name, navEmployee.Last_Name),
                passwordHash: string.Empty // Password not in NAV
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get employee from NAV: {EmployeeNumber}",
                employeeNumber);
            return null;
        }
    }
}
```

---

### 4. API Layer (REST Endpoints)

```csharp
// src/Api/ESSPortal.Api/Controllers/AuthController.cs
namespace ESSPortal.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly ICommandHandler<LoginCommand, LoginResult> _loginHandler;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ICommandHandler<LoginCommand, LoginResult> loginHandler,
        ILogger<AuthController> logger)
    {
        _loginHandler = loginHandler;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate a user and return JWT tokens
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Validate request
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Create command
        var command = new LoginCommand(
            request.EmailOrEmployeeNumber,
            request.Password,
            request.RememberMe);

        // Execute command
        var result = await _loginHandler.Handle(command, HttpContext.RequestAborted);

        // Handle result
        if (result.IsFailure)
        {
            _logger.LogWarning("Login failed: {Error}", result.Error);
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Authentication Failed",
                Detail = result.Error
            });
        }

        var loginResult = result.Value;

        // Requires 2FA
        if (loginResult.Requires2FA)
        {
            return Ok(new LoginResponse
            {
                Requires2FA = true,
                TwoFactorToken = loginResult.TwoFactorToken
            });
        }

        // Successful login
        return Ok(new LoginResponse
        {
            Requires2FA = false,
            AccessToken = loginResult.AccessToken!,
            RefreshToken = loginResult.RefreshToken!,
            Employee = loginResult.Employee!,
            ExpiresIn = 3600 // 1 hour
        });
    }
}


// src/Api/ESSPortal.Api/Contracts/LoginRequest.cs
public class LoginRequest
{
    [Required]
    public string EmailOrEmployeeNumber { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}


// src/Api/ESSPortal.Api/Contracts/LoginResponse.cs
public class LoginResponse
{
    public bool Requires2FA { get; init; }
    public string? TwoFactorToken { get; init; }

    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public int? ExpiresIn { get; init; }
    public EmployeeDto? Employee { get; init; }
}
```

---

### 5. MVC UI Layer (Web Application)

```csharp
// src/UI/ESSPortal.Web.Mvc/Controllers/AuthController.cs
namespace ESSPortal.Web.Mvc.Controllers;

public class AuthController : Controller
{
    private readonly ICommandHandler<LoginCommand, LoginResult> _loginHandler;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ICommandHandler<LoginCommand, LoginResult> loginHandler,
        ILogger<AuthController> logger)
    {
        _loginHandler = loginHandler;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult SignIn(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Create command (same as API!)
        var command = new LoginCommand(
            model.EmailOrEmployeeNumber,
            model.Password,
            model.RememberMe);

        // Execute command (IN-MEMORY CALL, not HTTP!)
        var result = await _loginHandler.Handle(command, HttpContext.RequestAborted);

        // Handle failure
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            return View(model);
        }

        var loginResult = result.Value;

        // Requires 2FA
        if (loginResult.Requires2FA)
        {
            TempData["TwoFactorToken"] = loginResult.TwoFactorToken;
            return RedirectToAction(nameof(TwoFactor), new { returnUrl });
        }

        // Sign in with cookie authentication
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, loginResult.Employee!.Id.ToString()),
            new(ClaimTypes.Email, loginResult.Employee.Email),
            new("employee_number", loginResult.Employee.EmployeeNumber),
            new(ClaimTypes.Name, loginResult.Employee.FullName)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = model.RememberMe
                ? DateTimeOffset.UtcNow.AddDays(30)
                : DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        _logger.LogInformation("User {Email} signed in successfully", loginResult.Employee.Email);

        // Redirect
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Dashboard");
    }
}


// src/UI/ESSPortal.Web.Mvc/ViewModels/Auth/SignInViewModel.cs
public class SignInViewModel
{
    [Required(ErrorMessage = "Email or Employee Number is required")]
    [Display(Name = "Email or Employee Number")]
    public string EmailOrEmployeeNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}
```

---

### 6. Dependency Injection Setup

```csharp
// src/Api/ESSPortal.Api/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add layers
builder.Services.AddDomain();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiPresentation();

var app = builder.Build();
app.Run();


// src/Application/ESSPortal.Application/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register all command handlers
        services.AddScoped<ICommandHandler<LoginCommand, LoginResult>, LoginCommandHandler>();
        services.AddScoped<ICommandHandler<RegisterEmployeeCommand, bool>, RegisterEmployeeCommandHandler>();

        // Or use reflection to auto-register
        services.Scan(scan => scan
            .FromAssemblyOf<ICommandHandler<,>>()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}


// src/Infrastructure/ESSPortal.Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ISessionManager, SessionManager>();
        services.AddScoped<IEmailService, EmailService>();

        // NAV Integration
        services.AddScoped<INavEmployeeAdapter, NavEmployeeAdapter>();
        services.AddNavIntegration(configuration);

        // Configuration
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<EmailSettings>(configuration.GetSection("Email"));

        return services;
    }
}
```

---

## Key Differences from Your Current Implementation

| Aspect | Your Current Code | Correct Architecture |
|--------|------------------|---------------------|
| **MVC → Business Logic** | HTTP call to API | Direct in-memory call |
| **Domain Model** | Uses NAV entities directly | Domain entities, NAV adapted |
| **Business Logic Location** | Scattered in services | Centralized in command handlers |
| **Validation** | Password checked in service | Domain entity validates itself |
| **Dependencies** | Domain depends on ASP.NET | Domain has zero dependencies |
| **Testability** | Hard (needs HTTP mocking) | Easy (pure business logic) |
| **Performance** | ~50-200ms (HTTP) | ~1-5ms (in-memory) |
| **Error Handling** | HTTP status codes + JSON | Result pattern |
| **Code Reuse** | Duplicate services | Single command handler |

---

## Flow Comparison

### Your Current Flow:
```
User → MVC Controller → ApiService (HTTP)
  → API Controller → AuthService (Infrastructure)
  → UserManager (Identity) → Database
```
**Issues**: 3 layers of indirection, network call, 2 auth systems

### Correct Flow:
```
User → MVC/API Controller → Command Handler (Application)
  → Domain Entity + Repository (Infrastructure) → Database
```
**Benefits**: Direct, fast, single auth system, shared logic

---

## Testing Comparison

### Your Current Code (Hard to Test):
```csharp
// Can't test without mocking HTTP client, API, database, etc.
[Fact]
public async Task Login_Should_ReturnToken()
{
    var httpMock = new Mock<HttpClient>();
    var apiServiceMock = new Mock<IApiService>();
    // ... 50 lines of setup

    var result = await controller.Login(request);
    // Brittle test
}
```

### Correct Architecture (Easy to Test):
```csharp
// Pure business logic test
[Fact]
public async Task Login_WithValidCredentials_Should_Succeed()
{
    // Arrange
    var employee = Employee.Register(
        new EmployeeNumber("EMP001"),
        new EmailAddress("test@example.com"),
        new FullName("John", "Doe"),
        "hashedpassword");

    var repoMock = new Mock<IEmployeeRepository>();
    repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<EmailAddress>()))
        .ReturnsAsync(employee);

    var handler = new LoginCommandHandler(repoMock.Object, /* ... */);

    // Act
    var result = await handler.Handle(
        new LoginCommand("test@example.com", "password", false));

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value.AccessToken);
}
```

---

## Migration Path

You can migrate incrementally:

1. **Create new command handlers** alongside existing code
2. **Update API to use handlers** (breaking change for API clients, but internal)
3. **Update MVC to use handlers directly** (remove HTTP calls)
4. **Delete old service layer** once everything migrated
5. **Update tests** to test command handlers

---

## Summary

**Your current approach**: Distributed system within a monolith
**Correct approach**: Clean separation of concerns with shared business logic

The correct architecture:
- ✅ Is faster (no HTTP overhead)
- ✅ Is simpler (no duplicate services)
- ✅ Is more maintainable (single source of truth)
- ✅ Is easier to test (pure business logic)
- ✅ Is more extensible (add features without touching multiple layers)
- ✅ Follows SOLID principles
- ✅ Is framework-agnostic (can swap ASP.NET for something else)

This is how Clean Architecture is supposed to work!
