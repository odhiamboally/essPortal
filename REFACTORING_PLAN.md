# Comprehensive Refactoring Plan for ESS Portal

## Executive Summary

This document outlines a phased approach to refactor the ESS Portal from its current problematic architecture to a maintainable, extensible Clean Architecture implementation.

**Estimated Effort**: 6-8 weeks (depending on team size)
**Risk Level**: Medium (can be done incrementally without breaking existing functionality)

---

## Current Problems (Recap)

1. **Broken Clean Architecture**: MVC app makes HTTP calls to API app (unnecessary network overhead)
2. **Domain Pollution**: Domain layer contains infrastructure concerns (NAV entities, ASP.NET Identity)
3. **Massive Duplication**: Duplicate enums, models, DTOs, and services across layers
4. **Misplaced Responsibilities**: Application layer doing too much (background services, encryption, caching)
5. **Tight Coupling to NAV**: Entire domain is NAV-specific, hard to test or swap
6. **Hardcoded Configuration**: Paths and settings embedded in code
7. **No Clear Bounded Contexts**: Everything mixed together

---

## Proposed Phases
  1. Phase 1 (Week 1-2): Foundation - Create true domain model, move NAV to infrastructure, consolidate duplicates
  2. Phase 2 (Week 3-4): Restructure Application layer with CQRS pattern
  3. Phase 3 (Week 5-6): Fix Presentation layer - remove MVC → API HTTP calls
  4. Phase 4 (Week 7): Configuration & deployment
  5. Phase 5 (Week 8): Testing & documentation
  6. Phase 6: Ongoing optimization

## Target Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Presentation Layer                       │
│  ┌────────────────────┐              ┌────────────────────┐     │
│  │   Web API Project   │              │   Web MVC Project   │    │
│  │  (REST Endpoints)   │              │  (Server-side UI)   │    │
│  └─────────┬───────────┘              └──────────┬─────────┘     │
└────────────┼─────────────────────────────────────┼───────────────┘
             │                                     │
             │         Share Application Layer     │
             └──────────────┬──────────────────────┘
                            │
┌────────────────────────────┼───────────────────────────────────┐
│                       Application Layer                         │
│  - Use Cases / Commands / Queries                               │
│  - Application Services (Orchestration only)                    │
│  - DTOs / ViewModels                                            │
│  - Interfaces (no implementations)                              │
└────────────────────────────┬───────────────────────────────────┘
                            │
┌────────────────────────────┼───────────────────────────────────┐
│                       Domain Layer                              │
│  - Core Business Entities (Employee, LeaveRequest, etc.)        │
│  - Value Objects                                                │
│  - Domain Events                                                │
│  - Business Rules & Invariants                                  │
│  - Repository Interfaces (no EF, no ASP.NET)                    │
└────────────────────────────┬───────────────────────────────────┘
                            │
┌────────────────────────────┼───────────────────────────────────┐
│                    Infrastructure Layer                         │
│  ┌───────────────┐  ┌──────────────┐  ┌──────────────┐        │
│  │ Persistence   │  │ Integration  │  │   Services    │        │
│  │ (EF Core)     │  │ (NAV/BC)     │  │ (Email, File) │        │
│  └───────────────┘  └──────────────┘  └──────────────┘        │
└─────────────────────────────────────────────────────────────────┘
```

---

## Phase 1: Foundation (Week 1-2)

### 1.1 Create True Domain Model

**Goal**: Establish your core business domain, independent of NAV

**Tasks**:
- [ ] Create new folder: `src/Domain/ESSPortal.Domain/Entities/Core/`
- [ ] Define core domain entities:
  - `Employee` (not EmployeeCard - that's NAV's model)
  - `LeaveRequest`
  - `LeaveType`
  - `LeavePeriod`
  - `LeaveBalance`
- [ ] Create Value Objects:
  - `EmployeeNumber`
  - `EmailAddress`
  - `DateRange`
  - `Money`
- [ ] Define domain enums (keep only business enums, not NAV status codes)
- [ ] Remove ASP.NET Identity references from Domain.csproj

**Files to Create**:
```
Domain/
├── Entities/
│   └── Core/
│       ├── Employee.cs
│       ├── LeaveRequest.cs
│       ├── LeaveType.cs
│       └── LeaveBalance.cs
├── ValueObjects/
│   ├── EmployeeNumber.cs
│   ├── EmailAddress.cs
│   └── DateRange.cs
└── Enums/
    ├── LeaveRequestStatus.cs (Open, PendingApproval, Approved, Rejected)
    ├── Gender.cs
    └── EmploymentType.cs
```

**Success Criteria**: Domain project has ZERO dependencies on any framework or external system

---

### 1.2 Move NAV Integration to Infrastructure

**Goal**: Isolate NAV-specific code from core domain

**Tasks**:
- [ ] Create: `src/Infrastructure/ESSPortal.Infrastructure.NAV/`
- [ ] Move all `NavEntities` from Domain to Infrastructure.NAV
- [ ] Create NAV adapter pattern:
  ```
  Infrastructure.NAV/
  ├── Models/           # NAV OData/SOAP models (old NavEntities)
  ├── Adapters/         # Translate NAV models → Domain entities
  │   ├── INavEmployeeAdapter.cs
  │   └── NavEmployeeAdapter.cs
  ├── Services/
  │   ├── NavODataClient.cs
  │   └── NavSoapClient.cs
  └── Configuration/
  ```
- [ ] Implement adapter pattern to map NAV → Domain

**Example Adapter**:
```csharp
public interface INavEmployeeAdapter
{
    Task<Employee?> GetEmployeeAsync(EmployeeNumber employeeNumber);
}

public class NavEmployeeAdapter : INavEmployeeAdapter
{
    private readonly INavODataClient _navClient;

    public async Task<Employee?> GetEmployeeAsync(EmployeeNumber employeeNumber)
    {
        var navEmployee = await _navClient.GetEmployeeCardAsync(employeeNumber.Value);
        if (navEmployee == null) return null;

        // Map NAV model → Domain model
        return new Employee(
            employeeNumber,
            new EmailAddress(navEmployee.Company_E_Mail),
            navEmployee.First_Name,
            navEmployee.Last_Name
            // ... etc
        );
    }
}
```

**Success Criteria**: Domain project has no references to NavEntities

---

### 1.3 Consolidate Duplicate Code

**Goal**: Single source of truth for all shared code

**Tasks**:
- [ ] Delete duplicate enums from UI project (use Domain enums)
- [ ] Delete duplicate models from UI project
- [ ] Create shared DTOs in Application layer only
- [ ] Update all references to use single source

**Files to Delete**:
```
src/UI/ESSPortal.Web.Mvc/Enums/NavEnums/        # DELETE ENTIRE FOLDER
src/UI/ESSPortal.Web.Mvc/Models/Navision/       # DELETE ENTIRE FOLDER
src/UI/ESSPortal.Web.Mvc/Dtos/                  # Move to Application if needed
```

**Success Criteria**: No duplicate enums or models across projects

---

## Phase 2: Restructure Application Layer (Week 3-4)

### 2.1 Implement CQRS Pattern

**Goal**: Separate read and write operations for clarity

**Tasks**:
- [ ] Create folder structure:
  ```
  Application/
  ├── Commands/
  │   ├── Auth/
  │   │   ├── RegisterEmployeeCommand.cs
  │   │   └── RegisterEmployeeCommandHandler.cs
  │   └── Leave/
  │       ├── CreateLeaveRequestCommand.cs
  │       └── CreateLeaveRequestCommandHandler.cs
  ├── Queries/
  │   ├── Auth/
  │   │   ├── GetUserQuery.cs
  │   │   └── GetUserQueryHandler.cs
  │   └── Leave/
  │       ├── GetLeaveBalanceQuery.cs
  │       └── GetLeaveBalanceQueryHandler.cs
  └── Interfaces/
      └── ICommandHandler.cs
      └── IQueryHandler.cs
  ```
- [ ] Implement command/query handlers
- [ ] Move business logic from services to handlers

**Example Command**:
```csharp
public record RegisterEmployeeCommand(
    string EmployeeNumber,
    string Email,
    string Password
);

public class RegisterEmployeeCommandHandler : ICommandHandler<RegisterEmployeeCommand, bool>
{
    private readonly INavEmployeeAdapter _navAdapter;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IPasswordHasher _passwordHasher;

    public async Task<Result<bool>> Handle(RegisterEmployeeCommand command)
    {
        // 1. Validate employee exists in NAV
        var navEmployee = await _navAdapter.GetEmployeeAsync(
            new EmployeeNumber(command.EmployeeNumber));

        if (navEmployee == null)
            return Result.Failure<bool>("Employee not found");

        // 2. Check if already registered
        var exists = await _employeeRepo.ExistsAsync(navEmployee.EmployeeNumber);
        if (exists)
            return Result.Failure<bool>("Already registered");

        // 3. Create domain entity
        var employee = Employee.Register(
            navEmployee.EmployeeNumber,
            navEmployee.Email,
            command.Password);

        // 4. Persist
        await _employeeRepo.AddAsync(employee);
        await _employeeRepo.SaveChangesAsync();

        return Result.Success(true);
    }
}
```

**Success Criteria**: All business operations are commands or queries

---

### 2.2 Move Infrastructure Concerns Out

**Goal**: Application layer should only orchestrate, not implement infrastructure

**Tasks**:
- [ ] Move `EncryptionService` from Application → Infrastructure
- [ ] Move `BackgroundServices` from Application → API/UI projects
- [ ] Move caching logic to Infrastructure
- [ ] Keep only interfaces in Application

**Moves**:
```
FROM: Application/Contracts/Implementations/Services/EncryptionService.cs
TO:   Infrastructure/Services/EncryptionService.cs

FROM: Application/BackgroundServices/
TO:   Api/BackgroundServices/
```

**Success Criteria**: Application project has no implementation files (only interfaces and DTOs)

---

## Phase 3: Fix Presentation Layer (Week 5-6)

### 3.1 Remove MVC → API HTTP Calls

**Goal**: MVC should use Application layer directly

**Tasks**:
- [ ] Delete entire `src/UI/ESSPortal.Web.Mvc/Contracts/Implementations/` folder
- [ ] MVC controllers inject Application services directly:
  ```csharp
  public class AuthController : Controller
  {
      private readonly ICommandHandler<RegisterEmployeeCommand, bool> _registerHandler;

      public AuthController(ICommandHandler<RegisterEmployeeCommand, bool> registerHandler)
      {
          _registerHandler = registerHandler;
      }

      [HttpPost]
      public async Task<IActionResult> Register(RegisterViewModel model)
      {
          var command = new RegisterEmployeeCommand(
              model.EmployeeNumber,
              model.Email,
              model.Password);

          var result = await _registerHandler.Handle(command);

          if (result.IsSuccess)
              return RedirectToAction("Success");

          ModelState.AddModelError("", result.Error);
          return View(model);
      }
  }
  ```
- [ ] Remove `ApiService`, `HttpClient` configurations from MVC
- [ ] Update DI registration in MVC Program.cs

**Files to Delete**:
```
src/UI/ESSPortal.Web.Mvc/Contracts/Implementations/Common/ApiService.cs
src/UI/ESSPortal.Web.Mvc/Contracts/Implementations/Services/*Service.cs (ALL)
```

**Success Criteria**: MVC project has NO HttpClient calls to API project

---

### 3.2 Decide on Deployment Model

**Option A: Shared Application Layer (Recommended)**
- MVC and API both reference Application layer
- Deploy as two separate apps
- MVC for human users, API for mobile/external integrations
- Share authentication via DataProtection

**Option B: Single Application**
- Merge API and MVC into one project
- API endpoints at `/api/*`
- MVC views at root
- Simpler deployment

**Option C: Full SPA**
- Keep API
- Replace MVC with React/Vue/Angular SPA
- True separation of concerns
- More complex but more scalable

**Decision Required**: Choose one approach and commit to it

---

## Phase 4: Configuration & Deployment (Week 7)

### 4.1 Externalize Configuration

**Goal**: No hardcoded values

**Tasks**:
- [ ] Move all hardcoded paths to `appsettings.json`:
  ```json
  {
    "DataProtection": {
      "KeyPath": "/app/keys",
      "ApplicationName": "ESSPortal"
    },
    "FileStorage": {
      "ProfilePicturesPath": "/app/uploads/profiles"
    },
    "NAV": {
      "ODataEndpoint": "https://nav.company.com/odata",
      "SoapEndpoint": "https://nav.company.com/soap"
    }
  }
  ```
- [ ] Update Program.cs to read from configuration
- [ ] Create environment-specific settings (Development, Staging, Production)
- [ ] Add configuration validation on startup

**Success Criteria**: Zero hardcoded paths or URLs in code

---

### 4.2 Improve Dependency Injection

**Tasks**:
- [ ] Create extension methods for clean DI registration:
  ```csharp
  // Infrastructure/Extensions/DependencyInjection.cs
  public static IServiceCollection AddInfrastructure(
      this IServiceCollection services,
      IConfiguration configuration)
  {
      services.AddScoped<IEmployeeRepository, EmployeeRepository>();
      services.AddScoped<INavEmployeeAdapter, NavEmployeeAdapter>();
      // ... etc
      return services;
  }
  ```
- [ ] Organize by feature/layer
- [ ] Remove service locator anti-pattern

**Success Criteria**: Clean, organized DI registration

---

## Phase 5: Testing & Documentation (Week 8)

### 5.1 Add Unit Tests

**Tasks**:
- [ ] Create test projects:
  ```
  tests/
  ├── Domain.Tests/
  ├── Application.Tests/
  └── Infrastructure.Tests/
  ```
- [ ] Test domain logic (entities, value objects)
- [ ] Test command/query handlers
- [ ] Test adapters
- [ ] Aim for 70%+ code coverage

---

### 5.2 Update Documentation

**Tasks**:
- [ ] Update CLAUDE.md with new architecture
- [ ] Document command/query patterns
- [ ] Document how to add new features
- [ ] Create architecture decision records (ADRs)

---

## Phase 6: Optimization (Ongoing)

### 6.1 Performance

- [ ] Add response caching
- [ ] Optimize database queries (N+1 problems)
- [ ] Add proper indexes
- [ ] Implement connection pooling

### 6.2 Observability

- [ ] Add structured logging throughout
- [ ] Add telemetry/metrics
- [ ] Add health checks for NAV connectivity
- [ ] Add application insights or similar

### 6.3 Security

- [ ] Security audit
- [ ] Penetration testing
- [ ] Update dependencies
- [ ] Review authentication flows

---

## Implementation Strategy

### Approach: Strangler Fig Pattern

Don't rewrite everything at once. Instead:

1. **Create new structure alongside old**
2. **Migrate one feature at a time**
3. **Delete old code only after new code is verified**
4. **Keep system running throughout**

### Feature Migration Order

1. ✅ **Authentication/Registration** (most critical, do first)
2. Leave Requests (high usage)
3. Profile Management
4. Dashboard
5. Payroll (least critical for daily ops)

### Weekly Goals

- **Week 1**: Domain model + NAV adapter
- **Week 2**: Consolidate duplicates
- **Week 3**: CQRS for Auth
- **Week 4**: CQRS for Leave
- **Week 5**: Remove MVC HTTP calls
- **Week 6**: Complete other features
- **Week 7**: Configuration & deployment prep
- **Week 8**: Testing & documentation

---

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Breaking existing functionality | Feature flags, parallel implementation |
| Team learning curve | Pair programming, code reviews, documentation |
| NAV integration breaks | Comprehensive adapter tests, contract testing |
| Timeline slippage | Prioritize phases 1-3, defer phase 6 |
| Deployment issues | Test in staging environment first |

---

## Success Metrics

- [ ] Zero duplicate code (same enum/model/service in multiple places)
- [ ] Domain project has zero framework dependencies
- [ ] MVC project has zero HTTP calls to API
- [ ] 70%+ test coverage
- [ ] All configuration externalized
- [ ] Can swap NAV for another ERP with < 1 week effort
- [ ] New features can be added without touching multiple layers

---

## Post-Refactoring Benefits

1. **Testability**: Can unit test business logic without database or NAV
2. **Maintainability**: Clear responsibilities, easy to find code
3. **Extensibility**: Can add features without massive refactoring
4. **Performance**: No unnecessary HTTP calls between MVC and API
5. **Team Velocity**: New developers productive faster
6. **Deployment**: Simpler architecture means simpler deployments

---

## Decision Log

| Decision | Chosen Option | Date | Rationale |
|----------|---------------|------|-----------|
| Presentation Layer | [TBD] | [Date] | [Reason] |
| CQRS Library | [MediatR/Custom] | [Date] | [Reason] |
| Testing Framework | [xUnit/NUnit] | [Date] | [Reason] |

---

## Next Steps

1. **Review this plan with the team**
2. **Make key decisions** (presentation layer approach, timeline)
3. **Set up feature branches** for parallel work
4. **Start Phase 1** with Domain model
5. **Schedule weekly check-ins** to track progress

---

## Questions?

Document any questions or concerns here as you go through the refactoring.
