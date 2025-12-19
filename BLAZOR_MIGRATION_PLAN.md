# Blazor Migration Plan: MVC to Blazor with MudBlazor

## Executive Summary

This document outlines the comprehensive migration strategy from the current ASP.NET Core MVC application to a modern Blazor application using MudBlazor, with aggressive replacement of JavaScript with C# code.

**Target Technology**: Blazor Interactive Server with MudBlazor
**Estimated Effort**: 8-10 weeks
**Risk Level**: Medium (incremental migration possible)
**JavaScript Reduction**: ~90% (from 3000+ lines to <300 lines)

---

## Table of Contents

1. [Technology Decisions](#1-technology-decisions)
2. [Architecture Overview](#2-architecture-overview)
3. [Migration Strategy](#3-migration-strategy)
4. [JavaScript Replacement Plan](#4-javascript-replacement-plan)
5. [Phase-by-Phase Implementation](#5-phase-by-phase-implementation)
6. [Component Structure](#6-component-structure)
7. [State Management](#7-state-management)
8. [Authentication & Authorization](#8-authentication--authorization)
9. [Session Management](#9-session-management)
10. [File Handling](#10-file-handling)
11. [Testing Strategy](#11-testing-strategy)
12. [Deployment & DevOps](#12-deployment--devops)
13. [Migration Checklist](#13-migration-checklist)

---

## 1. Technology Decisions

### 1.1 Core Technologies

| Component | Technology | Reasoning |
|-----------|-----------|-----------|
| **UI Framework** | Blazor Server (Interactive) | Real-time updates, server-side state, SignalR built-in |
| **UI Component Library** | MudBlazor 7.x | Modern Material Design, comprehensive components, minimal JS |
| **.NET Version** | .NET 10 | Latest features, performance improvements |
| **Rendering Mode** | InteractiveServer | Best for authenticated apps, real-time session management |
| **State Management** | Cascading Parameters + Scoped Services | Built-in Blazor state management |
| **Validation** | FluentValidation + Blazor Validation | Existing investment, server-side validation |
| **HTTP Client** | Direct Application Layer calls | No HTTP overhead (align with Clean Architecture) |

### 1.2 MudBlazor Components Mapping

| Current (Bootstrap + JS) | Blazor (MudBlazor) | JS Reduction |
|-------------------------|-------------------|--------------|
| Bootstrap Modals + JS | `MudDialog` | 100% |
| SweetAlert2 | `MudDialog` + `MudSnackbar` | 100% |
| Bootstrap Forms + Validation JS | `MudForm` + `MudTextField` | 95% |
| Custom Date Picker JS | `MudDatePicker` + C# | 100% |
| File Upload JS | `MudFileUpload` + C# | 95% |
| Toast Notifications | `MudSnackbar` | 100% |
| Session Timeout JS | Blazor `Timer` + C# | 100% |
| Bootstrap Cards | `MudCard` | 100% |
| Bootstrap Tables | `MudTable` or `MudDataGrid` | 100% |
| Custom Dropdowns | `MudSelect` + `MudAutocomplete` | 100% |

### 1.3 Minimal JavaScript Required

Only keep JavaScript for:
1. **File Downloads** - Browser download trigger (~10 lines)
2. **Local Storage** - Device fingerprinting if needed (~20 lines)
3. **Focus Management** - Accessibility edge cases (~10 lines)

**Total: ~40-50 lines** (down from 3000+)

---

## 2. Architecture Overview

### 2.1 Target Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Blazor Server Application                 │
│                  (ESSPortal.Web.Blazor)                      │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Pages (Routable Components)                         │   │
│  │  - /signin, /dashboard, /leave, /profile            │   │
│  └──────────────┬───────────────────────────────────────┘   │
│                 │                                            │
│  ┌──────────────▼───────────────────────────────────────┐   │
│  │  Shared Components                                   │   │
│  │  - Layouts, Dialogs, Cards, Forms                   │   │
│  └──────────────┬───────────────────────────────────────┘   │
│                 │                                            │
│  ┌──────────────▼───────────────────────────────────────┐   │
│  │  Services (Blazor-specific)                          │   │
│  │  - AuthStateProvider, DialogService, SnackbarService│   │
│  └──────────────┬───────────────────────────────────────┘   │
└─────────────────┼────────────────────────────────────────────┘
                  │
                  │ Direct In-Memory Calls
                  │
┌─────────────────▼────────────────────────────────────────────┐
│              Application Layer (CQRS)                         │
│  Commands/Queries, Handlers, DTOs                            │
└──────────────────────────────────────────────────────────────┘
                  │
┌─────────────────▼────────────────────────────────────────────┐
│              Domain Layer                                     │
│  Core Business Entities, Value Objects, Business Rules       │
└──────────────────────────────────────────────────────────────┘
                  │
┌─────────────────▼────────────────────────────────────────────┐
│              Infrastructure Layer                             │
│  Repositories, NAV Adapters, Email, File Storage             │
└──────────────────────────────────────────────────────────────┘
```

### 2.2 Project Structure

```
src/UI/ESSPortal.Web.Blazor/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   ├── AuthLayout.razor
│   │   └── NavMenu.razor
│   ├── Pages/
│   │   ├── Auth/
│   │   │   ├── SignIn.razor
│   │   │   ├── Register.razor
│   │   │   ├── TwoFactorLogin.razor
│   │   │   ├── ForgotPassword.razor
│   │   │   ├── ResetPassword.razor
│   │   │   └── Lock.razor
│   │   ├── Dashboard/
│   │   │   └── Index.razor
│   │   ├── Leave/
│   │   │   ├── ApplyForLeave.razor
│   │   │   ├── LeaveHistory.razor
│   │   │   └── EditLeave.razor
│   │   ├── Profile/
│   │   │   └── Index.razor
│   │   └── Payroll/
│   │       └── Index.razor
│   ├── Shared/
│   │   ├── Dialogs/
│   │   │   ├── EditLeaveDialog.razor
│   │   │   ├── LeaveDetailDialog.razor
│   │   │   ├── PayslipDialog.razor
│   │   │   ├── P9Dialog.razor
│   │   │   ├── PersonalDetailsDialog.razor
│   │   │   ├── ContactInfoDialog.razor
│   │   │   ├── BankingInfoDialog.razor
│   │   │   ├── ProfilePictureDialog.razor
│   │   │   ├── TwoFactorSettingsDialog.razor
│   │   │   └── SessionTimeoutDialog.razor
│   │   ├── Cards/
│   │   │   ├── LeaveApplicationCard.razor
│   │   │   ├── LeaveStatisticsCard.razor
│   │   │   └── QuickActionsCard.razor
│   │   └── Forms/
│   │       ├── LeaveApplicationForm.razor
│   │       ├── ProfileForm.razor
│   │       └── TwoFactorSetupForm.razor
│   └── _Imports.razor
├── Services/
│   ├── Auth/
│   │   ├── CustomAuthenticationStateProvider.cs
│   │   └── IAuthenticationService.cs
│   ├── Session/
│   │   ├── SessionManager.cs
│   │   ├── ISessionManager.cs
│   │   └── SessionState.cs
│   ├── UI/
│   │   ├── IDialogService.cs (wrapper for MudBlazor)
│   │   └── ISnackbarService.cs (wrapper for MudBlazor)
│   └── Interop/
│       └── FileDownloadService.cs (minimal JS)
├── Models/
│   └── UI/ (Blazor-specific ViewModels if needed)
├── wwwroot/
│   ├── css/
│   │   └── app.css (custom styles only)
│   ├── js/
│   │   └── interop.js (minimal JS - ~50 lines)
│   └── images/
├── Program.cs
└── appsettings.json
```

---

## 3. Migration Strategy

### 3.1 Migration Approach: Parallel Development

**Strategy**: Build Blazor app alongside MVC, migrate feature-by-feature, then switch over.

**Phases**:
1. **Foundation** (Week 1-2): Project setup, MudBlazor, authentication, layout
2. **Core Features** (Week 3-6): Dashboard, Leave, Profile, Payroll
3. **Advanced Features** (Week 7-8): Session management, 2FA, Lock screen
4. **Testing & Polish** (Week 9): Testing, bug fixes, performance
5. **Deployment** (Week 10): Production deployment, monitoring

### 3.2 Feature Migration Order

| Week | Feature | Priority | Complexity | Dependencies |
|------|---------|----------|------------|--------------|
| 1-2 | **Foundation Setup** | Critical | Medium | None |
| | - MudBlazor installation | | | |
| | - Layout components | | | |
| | - Authentication state | | | |
| | - Base navigation | | | |
| 3 | **Authentication** | Critical | High | Foundation |
| | - Sign In | | | |
| | - Sign Out | | | |
| | - Register | | | |
| | - Password Reset | | | |
| 4 | **Dashboard** | High | Medium | Auth |
| | - Leave statistics | | | |
| | - Quick actions | | | |
| | - Leave cards | | | |
| 5 | **Leave Management** | High | High | Auth, Dashboard |
| | - Apply for Leave | | | |
| | - Leave History | | | |
| | - Edit Leave | | | |
| | - File Upload | | | |
| 6 | **Profile Management** | Medium | Medium | Auth |
| | - View Profile | | | |
| | - Edit Personal Details | | | |
| | - Edit Contact Info | | | |
| | - Edit Banking Info | | | |
| | - Profile Picture Upload | | | |
| 6 | **Payroll** | Medium | Low | Auth |
| | - Payslip Generation | | | |
| | - P9 Generation | | | |
| 7 | **Two-Factor Auth** | High | High | Auth |
| | - 2FA Setup | | | |
| | - 2FA Login | | | |
| | - Backup Codes | | | |
| | - Settings | | | |
| 8 | **Session Management** | High | High | Auth |
| | - Idle Detection | | | |
| | - Keep-Alive | | | |
| | - Lock Screen | | | |
| | - Timeout Warning | | | |
| 9 | **Testing & Polish** | Critical | Medium | All |
| 10 | **Deployment** | Critical | Medium | All |

---

## 4. JavaScript Replacement Plan

### 4.1 Session Manager (597 lines JS → ~150 lines C#)

#### Current JavaScript Functionality:
```javascript
// session-manager.js (597 lines)
- Idle detection with debouncing
- Keep-alive polling (30s intervals)
- Countdown timer (30s warning)
- Lock screen trigger
- Activity tracking (mouse, keyboard, scroll)
- SweetAlert modal
```

#### Blazor C# Replacement:
```csharp
// Services/Session/SessionManager.cs
public class SessionManager : IAsyncDisposable
{
    private readonly IDialogService _dialogService;
    private readonly NavigationManager _navigationManager;
    private PeriodicTimer? _keepAliveTimer;
    private PeriodicTimer? _sessionCheckTimer;
    private DateTime _lastActivity;
    private bool _isWarningShown;

    // Configuration
    private const int SESSION_TIMEOUT_MINUTES = 15;
    private const int IDLE_TIMEOUT_MINUTES = 5;
    private const int WARNING_SECONDS = 30;
    private const int KEEP_ALIVE_SECONDS = 30;

    public async Task StartSessionMonitoring()
    {
        _lastActivity = DateTime.UtcNow;

        // Keep-alive timer (every 30s)
        _keepAliveTimer = new PeriodicTimer(TimeSpan.FromSeconds(KEEP_ALIVE_SECONDS));
        _ = Task.Run(async () =>
        {
            while (await _keepAliveTimer.WaitForNextTickAsync())
            {
                await SendKeepAliveAsync();
            }
        });

        // Session check timer (every 5s)
        _sessionCheckTimer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        _ = Task.Run(async () =>
        {
            while (await _sessionCheckTimer.WaitForNextTickAsync())
            {
                await CheckSessionTimeoutAsync();
            }
        });
    }

    public void RecordActivity()
    {
        _lastActivity = DateTime.UtcNow;
        _isWarningShown = false;
    }

    private async Task CheckSessionTimeoutAsync()
    {
        var idleTime = DateTime.UtcNow - _lastActivity;
        var timeUntilLock = TimeSpan.FromMinutes(IDLE_TIMEOUT_MINUTES) - idleTime;

        // Show warning 30 seconds before lock
        if (timeUntilLock.TotalSeconds <= WARNING_SECONDS && !_isWarningShown)
        {
            _isWarningShown = true;
            await ShowTimeoutWarningAsync(timeUntilLock);
        }

        // Lock screen if idle too long
        if (idleTime >= TimeSpan.FromMinutes(IDLE_TIMEOUT_MINUTES))
        {
            await LockScreenAsync();
        }
    }

    private async Task ShowTimeoutWarningAsync(TimeSpan remainingTime)
    {
        var parameters = new DialogParameters
        {
            { "RemainingSeconds", (int)remainingTime.TotalSeconds },
            { "OnContinue", new Action(RecordActivity) }
        };

        await _dialogService.ShowAsync<SessionTimeoutDialog>("Session Timeout Warning", parameters);
    }

    private async Task LockScreenAsync()
    {
        await StopSessionMonitoring();
        _navigationManager.NavigateTo("/auth/lock");
    }

    public async ValueTask DisposeAsync()
    {
        await StopSessionMonitoring();
    }
}
```

**Benefits**:
- ✅ Pure C# - no JavaScript
- ✅ Type-safe
- ✅ Testable
- ✅ Built-in Blazor lifecycle management
- ✅ Uses `PeriodicTimer` (modern .NET)
- ✅ MudDialog instead of SweetAlert

---

### 4.2 Modal Management (430 lines JS → 0 lines JS)

#### Current JavaScript:
```javascript
// dashboard-modals.js (430 lines)
- Fetch partial views
- Show Bootstrap modals
- Handle form submissions in modals
- Base64 file downloads
```

#### Blazor Replacement:
```razor
@* No JavaScript needed! *@
@inject IDialogService DialogService

<MudButton OnClick="OpenPayslipDialog">Generate Payslip</MudButton>

@code {
    private async Task OpenPayslipDialog()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        var dialog = await DialogService.ShowAsync<PayslipDialog>("Generate Payslip", options);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            // Handle result
        }
    }
}
```

**Benefits**:
- ✅ Zero JavaScript
- ✅ Component-based (reusable)
- ✅ Strong typing
- ✅ Built-in MudBlazor styling

---

### 4.3 Form Validation (Multiple files → 0 lines JS)

#### Current JavaScript:
```javascript
// login.js, register.js, leave-application.js, etc.
- Client-side validation
- Error message display
- Form submission handling
```

#### Blazor Replacement:
```razor
<EditForm Model="@model" OnValidSubmit="HandleSubmit">
    <FluentValidationValidator />

    <MudTextField @bind-Value="model.Email"
                  Label="Email"
                  For="@(() => model.Email)"
                  Immediate="true" />

    <MudTextField @bind-Value="model.Password"
                  Label="Password"
                  InputType="InputType.Password"
                  For="@(() => model.Password)"
                  Immediate="true" />

    <MudButton ButtonType="ButtonType.Submit"
               Variant="Variant.Filled"
               Color="Color.Primary">
        Sign In
    </MudButton>
</EditForm>

@code {
    private SignInModel model = new();

    private async Task HandleSubmit()
    {
        // Handle form submission
    }
}
```

**Benefits**:
- ✅ Zero JavaScript
- ✅ FluentValidation integration
- ✅ Real-time validation
- ✅ Accessible by default

---

### 4.4 Leave Application Form (1000+ lines JS → ~200 lines C#)

#### Current JavaScript:
```javascript
// leave-application.js (1000+ lines)
- Date picker initialization
- Days calculation based on dates
- Reliever dropdown population
- File upload handling
- Leave balance checking
- Form validation
- Half-day logic
```

#### Blazor Replacement:
```razor
@* LeaveApplicationForm.razor *@
<EditForm Model="@Model" OnValidSubmit="HandleSubmit">
    <FluentValidationValidator />

    <MudSelect @bind-Value="Model.LeaveTypeId"
               Label="Leave Type"
               For="@(() => Model.LeaveTypeId)">
        @foreach (var type in leaveTypes)
        {
            <MudSelectItem Value="@type.Id">@type.Name</MudSelectItem>
        }
    </MudSelect>

    <MudDatePicker @bind-Date="Model.StartDate"
                   Label="Start Date"
                   MinDate="@DateTime.Today"
                   DateChanged="@OnDateChanged"
                   For="@(() => Model.StartDate)" />

    <MudDatePicker @bind-Date="Model.EndDate"
                   Label="End Date"
                   MinDate="@Model.StartDate"
                   DateChanged="@OnDateChanged"
                   For="@(() => Model.EndDate)" />

    <MudCheckBox @bind-Value="Model.IsHalfDay"
                 Label="Half Day"
                 CheckedChanged="@OnHalfDayChanged" />

    <MudText Typo="Typo.body2">
        Total Days: @CalculateTotalDays()
    </MudText>

    <MudText Typo="Typo.body2" Color="@GetBalanceColor()">
        Available Balance: @GetAvailableBalance() days
    </MudText>

    <MudAutocomplete @bind-Value="Model.RelieverEmployeeNumber"
                     Label="Reliever"
                     SearchFunc="@SearchRelievers"
                     For="@(() => Model.RelieverEmployeeNumber)" />

    <MudFileUpload T="IReadOnlyList<IBrowserFile>"
                   FilesChanged="OnFilesChanged"
                   MaximumFileCount="10"
                   Accept=".pdf,.doc,.docx,.xls,.xlsx,.zip,.rar,.jpg,.png">
        <ActivatorContent>
            <MudButton Variant="Variant.Filled"
                       Color="Color.Primary"
                       StartIcon="@Icons.Material.Filled.CloudUpload">
                Upload Files
            </MudButton>
        </ActivatorContent>
    </MudFileUpload>

    @if (uploadedFiles.Any())
    {
        <MudList>
            @foreach (var file in uploadedFiles)
            {
                <MudListItem>
                    @file.Name (@FormatFileSize(file.Size))
                    <MudIconButton Icon="@Icons.Material.Filled.Delete"
                                   OnClick="@(() => RemoveFile(file))" />
                </MudListItem>
            }
        </MudList>
    }

    <MudButton ButtonType="ButtonType.Submit"
               Variant="Variant.Filled"
               Color="Color.Primary"
               Disabled="@(!IsFormValid())">
        Submit Application
    </MudButton>
</EditForm>

@code {
    [Parameter] public LeaveApplicationModel Model { get; set; } = new();

    private List<LeaveType> leaveTypes = new();
    private List<Employee> relievers = new();
    private List<IBrowserFile> uploadedFiles = new();

    private double CalculateTotalDays()
    {
        if (Model.StartDate == null || Model.EndDate == null)
            return 0;

        var days = (Model.EndDate.Value - Model.StartDate.Value).Days + 1;

        // Exclude weekends
        var totalDays = 0;
        for (var date = Model.StartDate.Value; date <= Model.EndDate.Value; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                totalDays++;
        }

        return Model.IsHalfDay ? 0.5 : totalDays;
    }

    private void OnDateChanged()
    {
        CalculateTotalDays();
        StateHasChanged();
    }

    private void OnHalfDayChanged(bool value)
    {
        if (value)
        {
            Model.EndDate = Model.StartDate;
        }
        StateHasChanged();
    }

    private async Task<IEnumerable<string>> SearchRelievers(string value, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Array.Empty<string>();

        var results = await LeaveService.SearchRelieversAsync(value, token);
        return results.Select(r => $"{r.EmployeeNumber} - {r.Name}");
    }

    private void OnFilesChanged(IReadOnlyList<IBrowserFile> files)
    {
        const long maxFileSize = 10 * 1024 * 1024; // 10MB

        foreach (var file in files)
        {
            if (file.Size > maxFileSize)
            {
                Snackbar.Add($"File {file.Name} exceeds 10MB limit", Severity.Error);
                continue;
            }

            uploadedFiles.Add(file);
        }
    }

    private void RemoveFile(IBrowserFile file)
    {
        uploadedFiles.Remove(file);
    }

    private double GetAvailableBalance()
    {
        var selectedType = leaveTypes.FirstOrDefault(t => t.Id == Model.LeaveTypeId);
        return selectedType?.AvailableBalance ?? 0;
    }

    private Color GetBalanceColor()
    {
        var balance = GetAvailableBalance();
        var requested = CalculateTotalDays();

        return balance >= requested ? Color.Success : Color.Error;
    }

    private bool IsFormValid()
    {
        return CalculateTotalDays() > 0
            && CalculateTotalDays() <= GetAvailableBalance()
            && !string.IsNullOrWhiteSpace(Model.RelieverEmployeeNumber);
    }

    private async Task HandleSubmit()
    {
        // Convert files to byte arrays
        var fileData = new List<FileUploadData>();
        foreach (var file in uploadedFiles)
        {
            using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            fileData.Add(new FileUploadData
            {
                FileName = file.Name,
                ContentType = file.ContentType,
                Data = ms.ToArray()
            });
        }

        Model.Attachments = fileData;

        // Submit to service
        var command = new CreateLeaveApplicationCommand
        {
            LeaveTypeId = Model.LeaveTypeId,
            StartDate = Model.StartDate.Value,
            EndDate = Model.EndDate.Value,
            IsHalfDay = Model.IsHalfDay,
            RelieverEmployeeNumber = Model.RelieverEmployeeNumber,
            Reason = Model.Reason,
            Attachments = fileData
        };

        var result = await Mediator.Send(command);

        if (result.IsSuccess)
        {
            Snackbar.Add("Leave application submitted successfully", Severity.Success);
            NavigationManager.NavigateTo("/leave/history");
        }
        else
        {
            Snackbar.Add(result.Error, Severity.Error);
        }
    }
}
```

**Benefits**:
- ✅ Zero JavaScript
- ✅ All logic in C#
- ✅ MudBlazor components (date picker, file upload, autocomplete)
- ✅ Real-time validation and balance checking
- ✅ Type-safe
- ✅ Testable business logic

---

### 4.5 Date Calculations

#### Current JavaScript:
```javascript
function calculateDays(startDate, endDate, excludeWeekends = true) {
    // Complex date logic
}

function calculateResumptionDate(endDate) {
    // Skip weekends
}
```

#### Blazor C# Replacement:
```csharp
public static class DateCalculations
{
    public static int CalculateWorkingDays(DateTime startDate, DateTime endDate)
    {
        var days = 0;
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Saturday &&
                date.DayOfWeek != DayOfWeek.Sunday)
            {
                days++;
            }
        }
        return days;
    }

    public static DateTime CalculateResumptionDate(DateTime endDate)
    {
        var resumptionDate = endDate.AddDays(1);

        while (resumptionDate.DayOfWeek == DayOfWeek.Saturday ||
               resumptionDate.DayOfWeek == DayOfWeek.Sunday)
        {
            resumptionDate = resumptionDate.AddDays(1);
        }

        return resumptionDate;
    }
}
```

---

### 4.6 File Downloads (Base64 PDF)

#### Current JavaScript:
```javascript
function downloadBase64File(base64, filename) {
    const blob = base64toBlob(base64, 'application/pdf');
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
}
```

#### Blazor Replacement (Minimal JS Required):
```javascript
// wwwroot/js/interop.js (one of the few JS we keep)
window.downloadFileFromStream = async (fileName, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.click();
    URL.revokeObjectURL(url);
};
```

```csharp
// Services/Interop/FileDownloadService.cs
public class FileDownloadService
{
    private readonly IJSRuntime _jsRuntime;

    public async Task DownloadFileAsync(string fileName, byte[] fileBytes)
    {
        using var streamRef = new DotNetStreamReference(new MemoryStream(fileBytes));
        await _jsRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }
}
```

**Note**: This is one of the few cases where we NEED JavaScript (browser limitation).

---

## 5. Phase-by-Phase Implementation

### Phase 1: Foundation Setup (Week 1-2)

#### Week 1: Project Setup & MudBlazor Integration

**Tasks**:
1. Install MudBlazor NuGet packages
2. Configure MudBlazor in Program.cs
3. Replace Bootstrap with MudBlazor theme
4. Create base layouts (MainLayout, AuthLayout)
5. Set up navigation menu with MudNavMenu
6. Configure routing

**Deliverables**:
- [ ] MudBlazor installed and configured
- [ ] Base layout components created
- [ ] Navigation working
- [ ] Theme customization applied

**Code Example - Program.cs**:
```csharp
using MudBlazor.Services;
using ESSPortal.Web.Blazor.Components;
using ESSPortal.Web.Blazor.Services.Auth;
using ESSPortal.Web.Blazor.Services.Session;

var builder = WebApplication.CreateBuilder(args);

// Add Blazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MudBlazor services
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
});

// Add authentication
builder.Services.AddAuthenticationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthenticationStateProvider>());

// Add session management
builder.Services.AddScoped<ISessionManager, SessionManager>();

// Add Application layer (CQRS handlers, etc.)
builder.Services.AddApplication();

// Add Infrastructure layer (repositories, NAV adapters, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Add minimal JavaScript interop
builder.Services.AddScoped<FileDownloadService>();

var app = builder.Build();

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

---

#### Week 2: Authentication State & Authorization

**Tasks**:
1. Create CustomAuthenticationStateProvider
2. Implement token storage (in-memory or Redis)
3. Create authorization policies
4. Add [Authorize] attribute support
5. Create RedirectToLogin component
6. Set up cascading authentication state

**Deliverables**:
- [ ] AuthenticationStateProvider implemented
- [ ] Authorization working
- [ ] Redirect to login for unauthorized access
- [ ] User claims available throughout app

**Code Example - CustomAuthenticationStateProvider.cs**:
```csharp
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ISessionStorage _sessionStorage;
    private readonly NavigationManager _navigationManager;

    public CustomAuthenticationStateProvider(
        ISessionStorage sessionStorage,
        NavigationManager navigationManager)
    {
        _sessionStorage = sessionStorage;
        _navigationManager = navigationManager;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _sessionStorage.GetTokenAsync();

        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }

    public async Task MarkUserAsAuthenticated(string token, IEnumerable<Claim> claims)
    {
        await _sessionStorage.SetTokenAsync(token);

        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _sessionStorage.RemoveTokenAsync();

        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty));
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
```

---

### Phase 2: Core Features (Week 3-6)

#### Week 3: Authentication Pages

**Features to Implement**:
1. Sign In page
2. Register page
3. Forgot Password page
4. Reset Password page
5. Sign Out functionality

**Components**:
- `Pages/Auth/SignIn.razor`
- `Pages/Auth/Register.razor`
- `Pages/Auth/ForgotPassword.razor`
- `Pages/Auth/ResetPassword.razor`

**Example - SignIn.razor**:
```razor
@page "/signin"
@layout AuthLayout
@inject ICommandHandler<LoginCommand, LoginResult> LoginHandler
@inject CustomAuthenticationStateProvider AuthStateProvider
@inject NavigationManager NavigationManager
@inject ISnackbar Snackbar

<MudContainer MaxWidth="MaxWidth.Small" Class="mt-8">
    <MudPaper Elevation="4" Class="pa-8">
        <MudText Typo="Typo.h4" Align="Align.Center" GutterBottom="true">
            ESS Portal
        </MudText>
        <MudText Typo="Typo.body1" Align="Align.Center" Class="mb-6">
            Sign in to your account
        </MudText>

        <EditForm Model="@model" OnValidSubmit="HandleSignIn">
            <FluentValidationValidator />

            <MudTextField @bind-Value="model.EmailOrEmployeeNumber"
                          Label="Email or Employee Number"
                          Variant="Variant.Outlined"
                          For="@(() => model.EmailOrEmployeeNumber)"
                          Immediate="true"
                          FullWidth="true" />

            <MudTextField @bind-Value="model.Password"
                          Label="Password"
                          Variant="Variant.Outlined"
                          InputType="@passwordInputType"
                          For="@(() => model.Password)"
                          Immediate="true"
                          FullWidth="true"
                          Adornment="Adornment.End"
                          AdornmentIcon="@passwordIcon"
                          OnAdornmentClick="TogglePasswordVisibility" />

            <MudCheckBox @bind-Value="model.RememberMe"
                         Label="Remember me"
                         Color="Color.Primary" />

            <MudButton ButtonType="ButtonType.Submit"
                       Variant="Variant.Filled"
                       Color="Color.Primary"
                       FullWidth="true"
                       Disabled="@isLoading"
                       Class="mt-4">
                @if (isLoading)
                {
                    <MudProgressCircular Size="Size.Small" Indeterminate="true" />
                }
                else
                {
                    <span>Sign In</span>
                }
            </MudButton>
        </EditForm>

        <MudDivider Class="my-4" />

        <MudStack Row="true" Justify="Justify.SpaceBetween">
            <MudLink Href="/auth/forgot-password">Forgot Password?</MudLink>
            <MudLink Href="/auth/register">Create Account</MudLink>
        </MudStack>
    </MudPaper>
</MudContainer>

@code {
    private SignInModel model = new();
    private bool isLoading = false;
    private bool showPassword = false;

    private InputType passwordInputType => showPassword ? InputType.Text : InputType.Password;
    private string passwordIcon => showPassword ? Icons.Material.Filled.Visibility : Icons.Material.Filled.VisibilityOff;

    [SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

    private void TogglePasswordVisibility()
    {
        showPassword = !showPassword;
    }

    private async Task HandleSignIn()
    {
        isLoading = true;

        try
        {
            var command = new LoginCommand(
                model.EmailOrEmployeeNumber,
                model.Password,
                model.RememberMe);

            var result = await LoginHandler.Handle(command);

            if (result.IsFailure)
            {
                Snackbar.Add(result.Error, Severity.Error);
                return;
            }

            var loginResult = result.Value;

            // Check if 2FA required
            if (loginResult.Requires2FA)
            {
                // Store 2FA token and redirect
                NavigationManager.NavigateTo($"/auth/two-factor?token={loginResult.TwoFactorToken}");
                return;
            }

            // Update auth state
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, loginResult.Employee.Id.ToString()),
                new Claim(ClaimTypes.Email, loginResult.Employee.Email),
                new Claim("employee_number", loginResult.Employee.EmployeeNumber),
                new Claim(ClaimTypes.Name, loginResult.Employee.FullName)
            };

            await AuthStateProvider.MarkUserAsAuthenticated(loginResult.AccessToken, claims);

            Snackbar.Add("Successfully signed in!", Severity.Success);

            // Redirect
            var redirectUrl = !string.IsNullOrEmpty(ReturnUrl) ? ReturnUrl : "/dashboard";
            NavigationManager.NavigateTo(redirectUrl);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"An error occurred: {ex.Message}", Severity.Error);
        }
        finally
        {
            isLoading = false;
        }
    }

    public class SignInModel
    {
        public string EmailOrEmployeeNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
```

**Zero JavaScript Required!** 🎉

---

#### Week 4: Dashboard

**Features to Implement**:
1. Leave statistics cards
2. Quick action buttons
3. Leave application cards (recent applications)
4. Leave history section

**Components**:
- `Pages/Dashboard/Index.razor`
- `Shared/Cards/LeaveStatisticsCard.razor`
- `Shared/Cards/LeaveApplicationCard.razor`
- `Shared/Cards/QuickActionsCard.razor`

**Example - Dashboard/Index.razor**:
```razor
@page "/dashboard"
@attribute [Authorize]
@inject IQueryHandler<GetDashboardDataQuery, DashboardData> DashboardQuery
@inject IDialogService DialogService
@inject NavigationManager NavigationManager

<MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="mt-4">
    <MudText Typo="Typo.h4" GutterBottom="true">
        Welcome, @userName!
    </MudText>

    @if (isLoading)
    {
        <MudProgressLinear Indeterminate="true" />
    }
    else if (dashboardData != null)
    {
        <MudGrid>
            <!-- Leave Statistics -->
            <MudItem xs="12" md="4">
                <LeaveStatisticsCard Statistics="@dashboardData.LeaveStatistics" />
            </MudItem>

            <!-- Quick Actions -->
            <MudItem xs="12" md="4">
                <QuickActionsCard OnApplyLeave="NavigateToApplyLeave"
                                  OnGeneratePayslip="OpenPayslipDialog"
                                  OnGenerateP9="OpenP9Dialog" />
            </MudItem>

            <!-- Leave Balance -->
            <MudItem xs="12" md="4">
                <MudCard>
                    <MudCardHeader>
                        <CardHeaderContent>
                            <MudText Typo="Typo.h6">Leave Balance</MudText>
                        </CardHeaderContent>
                    </MudCardHeader>
                    <MudCardContent>
                        @foreach (var balance in dashboardData.LeaveBalances)
                        {
                            <MudStack Row="true" Justify="Justify.SpaceBetween" Class="mb-2">
                                <MudText>@balance.LeaveTypeName</MudText>
                                <MudChip Size="Size.Small" Color="Color.Primary">
                                    @balance.AvailableDays days
                                </MudChip>
                            </MudStack>
                        }
                    </MudCardContent>
                </MudCard>
            </MudItem>

            <!-- Recent Leave Applications -->
            <MudItem xs="12">
                <MudText Typo="Typo.h5" Class="mb-4">Recent Applications</MudText>
                <MudGrid>
                    @foreach (var application in dashboardData.RecentApplications)
                    {
                        <MudItem xs="12" md="6" lg="4">
                            <LeaveApplicationCard Application="@application"
                                                  OnView="@(() => ViewLeaveDetails(application.Id))"
                                                  OnEdit="@(() => EditLeave(application.Id))" />
                        </MudItem>
                    }
                </MudGrid>
            </MudItem>
        </MudGrid>
    }
</MudContainer>

@code {
    [CascadingParameter]
    private Task<AuthenticationState> authenticationState { get; set; }

    private DashboardData? dashboardData;
    private string userName = string.Empty;
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        var authState = await authenticationState;
        userName = authState.User.Identity?.Name ?? "User";

        await LoadDashboardData();
    }

    private async Task LoadDashboardData()
    {
        isLoading = true;

        try
        {
            var query = new GetDashboardDataQuery();
            var result = await DashboardQuery.Handle(query);

            if (result.IsSuccess)
            {
                dashboardData = result.Value;
            }
        }
        finally
        {
            isLoading = false;
        }
    }

    private void NavigateToApplyLeave()
    {
        NavigationManager.NavigateTo("/leave/apply");
    }

    private async Task OpenPayslipDialog()
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };
        await DialogService.ShowAsync<PayslipDialog>("Generate Payslip", options);
    }

    private async Task OpenP9Dialog()
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };
        await DialogService.ShowAsync<P9Dialog>("Generate P9", options);
    }

    private async Task ViewLeaveDetails(Guid leaveId)
    {
        var parameters = new DialogParameters { { "LeaveId", leaveId } };
        var options = new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true };
        await DialogService.ShowAsync<LeaveDetailDialog>("Leave Details", parameters, options);
    }

    private void EditLeave(Guid leaveId)
    {
        NavigationManager.NavigateTo($"/leave/edit/{leaveId}");
    }
}
```

---

#### Week 5: Leave Management

**Features to Implement**:
1. Apply for Leave page (with file upload)
2. Leave History page
3. Edit Leave page/dialog
4. Leave Detail dialog

**Components**:
- `Pages/Leave/ApplyForLeave.razor`
- `Pages/Leave/LeaveHistory.razor`
- `Shared/Dialogs/EditLeaveDialog.razor`
- `Shared/Dialogs/LeaveDetailDialog.razor`
- `Shared/Forms/LeaveApplicationForm.razor` (reusable)

**Key Features**:
- Date pickers (MudDatePicker)
- File upload (MudFileUpload)
- Reliever autocomplete (MudAutocomplete)
- Real-time days calculation (C#)
- Leave balance validation (C#)
- Half-day logic (C#)

---

#### Week 6: Profile & Payroll

**Profile Features**:
1. View profile page
2. Edit personal details dialog
3. Edit contact info dialog
4. Edit banking info dialog
5. Profile picture upload dialog
6. Profile completion percentage

**Payroll Features**:
1. Payslip generation dialog
2. P9 generation dialog
3. PDF download (using FileDownloadService)

**Components**:
- `Pages/Profile/Index.razor`
- `Shared/Dialogs/PersonalDetailsDialog.razor`
- `Shared/Dialogs/ContactInfoDialog.razor`
- `Shared/Dialogs/BankingInfoDialog.razor`
- `Shared/Dialogs/ProfilePictureDialog.razor`
- `Shared/Dialogs/PayslipDialog.razor`
- `Shared/Dialogs/P9Dialog.razor`

---

### Phase 3: Advanced Features (Week 7-8)

#### Week 7: Two-Factor Authentication

**Features to Implement**:
1. 2FA setup page (with QR code)
2. 2FA login page (code entry)
3. Backup codes display
4. 2FA settings dialog (enable/disable)

**Components**:
- `Pages/Auth/TwoFactorSetup.razor`
- `Pages/Auth/TwoFactorLogin.razor`
- `Pages/Auth/BackupCodes.razor`
- `Shared/Dialogs/TwoFactorSettingsDialog.razor`

**QR Code Generation**:
```csharp
// Use QRCoder NuGet package
public static class QRCodeGenerator
{
    public static string GenerateQRCodeImage(string totpUri)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(totpUri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCode.GetGraphic(20);

        return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
    }
}
```

**Usage in Blazor**:
```razor
<MudImage Src="@qrCodeImageUrl" Alt="QR Code" Width="200" Height="200" />
```

No JavaScript needed for QR code generation! ✅

---

#### Week 8: Session Management & Lock Screen

**Features to Implement**:
1. Idle detection (using Blazor circuit events)
2. Session timeout warning dialog (with countdown)
3. Keep-alive mechanism (PeriodicTimer)
4. Lock screen page
5. Unlock functionality

**Components**:
- `Services/Session/SessionManager.cs` (see Section 4.1)
- `Shared/Dialogs/SessionTimeoutDialog.razor`
- `Pages/Auth/Lock.razor`

**SessionManager Integration**:
```razor
@* MainLayout.razor *@
@inject ISessionManager SessionManager
@implements IDisposable

@code {
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await SessionManager.StartSessionMonitoring();
        }
    }

    public void Dispose()
    {
        SessionManager?.DisposeAsync();
    }
}
```

**Activity Tracking** (No JavaScript!):
```razor
@* Wrap entire app content *@
<div @onclick="RecordActivity"
     @onkeydown="RecordActivity"
     @onmousemove="RecordActivity"
     @onscroll="RecordActivity">
    @Body
</div>

@code {
    private void RecordActivity()
    {
        SessionManager.RecordActivity();
    }
}
```

**Session Timeout Dialog with Countdown**:
```razor
@* SessionTimeoutDialog.razor *@
<MudDialog>
    <DialogContent>
        <MudText Typo="Typo.h6" Class="mb-4">Session Timeout Warning</MudText>
        <MudText>
            Your session will expire in <strong>@remainingSeconds</strong> seconds due to inactivity.
        </MudText>
        <MudText Class="mt-2">
            Click "Continue" to extend your session.
        </MudText>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Logout</MudButton>
        <MudButton Color="Color.Primary" Variant="Variant.Filled" OnClick="ContinueSession">
            Continue
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public int RemainingSeconds { get; set; }
    [Parameter] public Action OnContinue { get; set; }

    private int remainingSeconds;
    private Timer? countdownTimer;

    protected override void OnInitialized()
    {
        remainingSeconds = RemainingSeconds;

        countdownTimer = new Timer(UpdateCountdown, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    private void UpdateCountdown(object? state)
    {
        remainingSeconds--;

        if (remainingSeconds <= 0)
        {
            InvokeAsync(() => MudDialog.Close());
        }
        else
        {
            InvokeAsync(StateHasChanged);
        }
    }

    private void ContinueSession()
    {
        OnContinue?.Invoke();
        MudDialog.Close();
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    public void Dispose()
    {
        countdownTimer?.Dispose();
    }
}
```

**Pure C# countdown timer!** No JavaScript! ✅

---

### Phase 4: Testing & Polish (Week 9)

**Testing Tasks**:
1. Unit tests for business logic
2. Integration tests for components
3. End-to-end tests (bUnit or Playwright)
4. Performance testing
5. Accessibility testing (MudBlazor has good a11y out of the box)
6. Cross-browser testing

**Polish Tasks**:
1. Error handling improvements
2. Loading states refinement
3. Responsive design verification
4. Theme customization
5. Animations and transitions
6. User feedback improvements

---

### Phase 5: Deployment (Week 10)

**Deployment Checklist**:
- [ ] Configure production appsettings
- [ ] Set up Application Insights / logging
- [ ] Configure Redis for session storage
- [ ] Set up health checks
- [ ] Configure HTTPS and certificates
- [ ] Set up SignalR scaling (if needed)
- [ ] Database migration scripts
- [ ] Deployment pipeline (CI/CD)
- [ ] Smoke tests
- [ ] User acceptance testing
- [ ] Documentation
- [ ] Training materials

---

## 6. Component Structure

### 6.1 Reusable Component Examples

#### LeaveApplicationCard.razor
```razor
<MudCard>
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">@Application.LeaveTypeName</MudText>
            <MudText Typo="Typo.body2">@Application.StartDate.ToShortDateString() - @Application.EndDate.ToShortDateString()</MudText>
        </CardHeaderContent>
        <CardHeaderActions>
            <MudChip Size="Size.Small" Color="@GetStatusColor()">
                @Application.Status
            </MudChip>
        </CardHeaderActions>
    </MudCardHeader>
    <MudCardContent>
        <MudText Typo="Typo.body2">
            <strong>Duration:</strong> @Application.TotalDays days
        </MudText>
        <MudText Typo="Typo.body2">
            <strong>Reliever:</strong> @Application.RelieverName
        </MudText>
        @if (!string.IsNullOrEmpty(Application.Reason))
        {
            <MudText Typo="Typo.body2" Class="mt-2">
                @Application.Reason
            </MudText>
        }
    </MudCardContent>
    <MudCardActions>
        <MudButton StartIcon="@Icons.Material.Filled.Visibility"
                   OnClick="@(() => OnView.InvokeAsync())">
            View
        </MudButton>
        @if (Application.Status == "Pending")
        {
            <MudButton StartIcon="@Icons.Material.Filled.Edit"
                       OnClick="@(() => OnEdit.InvokeAsync())">
                Edit
            </MudButton>
        }
    </MudCardActions>
</MudCard>

@code {
    [Parameter] public LeaveApplicationDto Application { get; set; } = null!;
    [Parameter] public EventCallback OnView { get; set; }
    [Parameter] public EventCallback OnEdit { get; set; }

    private Color GetStatusColor()
    {
        return Application.Status switch
        {
            "Approved" => Color.Success,
            "Pending" => Color.Warning,
            "Rejected" => Color.Error,
            _ => Color.Default
        };
    }
}
```

---

## 7. State Management

### 7.1 Scoped Services Pattern

For complex state that needs to be shared across components:

```csharp
// Services/State/LeaveApplicationState.cs
public class LeaveApplicationState
{
    private LeaveApplicationModel? _currentApplication;

    public event Action? OnChange;

    public LeaveApplicationModel? CurrentApplication
    {
        get => _currentApplication;
        set
        {
            _currentApplication = value;
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
```

**Registration**:
```csharp
builder.Services.AddScoped<LeaveApplicationState>();
```

**Usage**:
```razor
@inject LeaveApplicationState AppState
@implements IDisposable

@code {
    protected override void OnInitialized()
    {
        AppState.OnChange += StateHasChanged;
    }

    public void Dispose()
    {
        AppState.OnChange -= StateHasChanged;
    }
}
```

---

### 7.2 Cascading Parameters for User Context

```razor
@* App.razor *@
<CascadingAuthenticationState>
    <CascadingValue Value="@userContext">
        <Router AppAssembly="@typeof(App).Assembly">
            <!-- Router config -->
        </Router>
    </CascadingValue>
</CascadingAuthenticationState>

@code {
    private UserContext userContext = new();

    protected override async Task OnInitializedAsync()
    {
        // Load user context from auth state
    }
}
```

**Usage in any component**:
```razor
@code {
    [CascadingParameter]
    public UserContext UserContext { get; set; } = null!;
}
```

---

## 8. Authentication & Authorization

### 8.1 Protected Routes

```razor
@page "/dashboard"
@attribute [Authorize]
```

### 8.2 Role-Based Authorization

```razor
@attribute [Authorize(Roles = "Manager,Admin")]
```

### 8.3 Programmatic Authorization

```razor
<AuthorizeView Roles="Manager">
    <Authorized>
        <MudButton>Approve Leave</MudButton>
    </Authorized>
    <NotAuthorized>
        <MudText>You don't have permission to approve leave.</MudText>
    </NotAuthorized>
</AuthorizeView>
```

---

## 9. Session Management

### 9.1 SignalR Circuit Considerations

Blazor Server uses SignalR circuits for each user connection. Important considerations:

1. **Circuit lifetime** = User session lifetime
2. **Disconnection handling**: MudBlazor has built-in reconnect UI
3. **Scalability**: Use Redis backplane for multi-server deployments
4. **Memory**: Each circuit holds state in memory

### 9.2 Session Storage Options

| Storage | Use Case | Pros | Cons |
|---------|----------|------|------|
| In-Memory | Single server, dev | Fast, simple | Not scalable |
| Redis | Production, multi-server | Scalable, persistent | Additional infrastructure |
| SQL Server | Enterprise | Durable | Slower |

**Recommended**: Redis for production

---

## 10. File Handling

### 10.1 File Upload (MudBlazor)

```razor
<MudFileUpload T="IReadOnlyList<IBrowserFile>"
               FilesChanged="OnFilesChanged"
               MaximumFileCount="10"
               Accept=".pdf,.doc,.docx">
    <ActivatorContent>
        <MudButton>Upload Files</MudButton>
    </ActivatorContent>
</MudFileUpload>

@code {
    private async Task OnFilesChanged(IReadOnlyList<IBrowserFile> files)
    {
        const long maxFileSize = 10 * 1024 * 1024;

        foreach (var file in files)
        {
            if (file.Size > maxFileSize)
            {
                Snackbar.Add($"{file.Name} exceeds size limit", Severity.Error);
                continue;
            }

            using var stream = file.OpenReadStream(maxAllowedSize: maxFileSize);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            // Process file bytes
            var fileBytes = ms.ToArray();
        }
    }
}
```

### 10.2 File Download

```csharp
// Minimal JavaScript required (browser limitation)
await FileDownloadService.DownloadFileAsync("payslip.pdf", pdfBytes);
```

---

## 11. Testing Strategy

### 11.1 Unit Tests (xUnit + bUnit)

```csharp
public class LeaveApplicationFormTests
{
    [Fact]
    public void CalculateTotalDays_ExcludesWeekends()
    {
        // Arrange
        using var ctx = new TestContext();
        var component = ctx.RenderComponent<LeaveApplicationForm>();

        // Act
        component.Instance.Model.StartDate = new DateTime(2024, 1, 1); // Monday
        component.Instance.Model.EndDate = new DateTime(2024, 1, 5);   // Friday
        var totalDays = component.Instance.CalculateTotalDays();

        // Assert
        Assert.Equal(5, totalDays); // 5 weekdays
    }
}
```

### 11.2 Integration Tests

```csharp
[Fact]
public async Task SignIn_WithValidCredentials_ShouldSucceed()
{
    // Arrange
    using var ctx = new TestContext();
    ctx.Services.AddScoped<ICommandHandler<LoginCommand, LoginResult>>(sp => mockLoginHandler.Object);

    var component = ctx.RenderComponent<SignIn>();

    // Act
    component.Find("input[id='emailOrEmployeeNumber']").Change("test@example.com");
    component.Find("input[id='password']").Change("Password123!");
    component.Find("button[type='submit']").Click();

    // Assert
    Assert.Contains("Successfully signed in", component.Markup);
}
```

---

## 12. Deployment & DevOps

### 12.1 Production Configuration

```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...",
    "Redis": "redis-server:6379"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "CircuitOptions": {
    "DisconnectedCircuitRetentionPeriod": "00:03:00",
    "DisconnectedCircuitMaxRetained": 100
  },
  "SignalR": {
    "MaximumReceiveMessageSize": 32000,
    "HandshakeTimeout": "00:00:15"
  }
}
```

### 12.2 Docker Deployment

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/UI/ESSPortal.Web.Blazor/ESSPortal.Web.Blazor.csproj", "src/UI/ESSPortal.Web.Blazor/"]
RUN dotnet restore
COPY . .
WORKDIR "/src/src/UI/ESSPortal.Web.Blazor"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ESSPortal.Web.Blazor.dll"]
```

---

## 13. Migration Checklist

### 13.1 Foundation
- [ ] Install MudBlazor NuGet packages
- [ ] Configure MudBlazor in Program.cs
- [ ] Create MainLayout with MudLayout components
- [ ] Create AuthLayout
- [ ] Set up navigation menu
- [ ] Configure routing
- [ ] Set up CustomAuthenticationStateProvider
- [ ] Configure authorization policies

### 13.2 Authentication
- [ ] Sign In page
- [ ] Register page
- [ ] Forgot Password page
- [ ] Reset Password page
- [ ] Sign Out functionality
- [ ] Token storage
- [ ] Claims management

### 13.3 Dashboard
- [ ] Dashboard page
- [ ] Leave statistics card
- [ ] Quick actions card
- [ ] Leave balance display
- [ ] Recent applications list

### 13.4 Leave Management
- [ ] Apply for Leave page
- [ ] Leave application form with:
  - [ ] Date pickers
  - [ ] Reliever autocomplete
  - [ ] File upload
  - [ ] Days calculation
  - [ ] Balance validation
  - [ ] Half-day logic
- [ ] Leave History page
- [ ] Edit Leave dialog
- [ ] Leave Detail dialog

### 13.5 Profile
- [ ] Profile view page
- [ ] Personal details dialog
- [ ] Contact info dialog
- [ ] Banking info dialog
- [ ] Profile picture upload dialog
- [ ] Profile completion percentage

### 13.6 Payroll
- [ ] Payslip generation dialog
- [ ] P9 generation dialog
- [ ] PDF download functionality

### 13.7 Two-Factor Auth
- [ ] 2FA setup page
- [ ] QR code generation
- [ ] 2FA login page
- [ ] Backup codes page
- [ ] 2FA settings dialog

### 13.8 Session Management
- [ ] SessionManager service
- [ ] Idle detection
- [ ] Keep-alive mechanism
- [ ] Timeout warning dialog
- [ ] Lock screen page
- [ ] Unlock functionality
- [ ] Activity tracking

### 13.9 Testing
- [ ] Unit tests for business logic
- [ ] Component tests (bUnit)
- [ ] Integration tests
- [ ] End-to-end tests
- [ ] Performance tests
- [ ] Accessibility tests

### 13.10 Deployment
- [ ] Production configuration
- [ ] Redis setup
- [ ] Logging configuration
- [ ] Health checks
- [ ] Docker setup
- [ ] CI/CD pipeline
- [ ] Documentation

---

## Summary

This migration plan provides a comprehensive roadmap for migrating from ASP.NET Core MVC to Blazor Server with MudBlazor. The key achievements:

1. **~90% JavaScript Reduction** (from 3000+ to <50 lines)
2. **Modern UI** with Material Design (MudBlazor)
3. **Type-Safe** - Everything in C#
4. **Clean Architecture** - Direct Application layer calls
5. **Testable** - Unit and integration tests
6. **Maintainable** - Component-based architecture
7. **Real-time** - SignalR built-in
8. **Scalable** - Redis-backed sessions

**Total Effort**: 8-10 weeks
**Risk**: Medium (incremental migration)
**ROI**: High (better performance, maintainability, developer experience)

This is an aggressive but achievable migration that will modernize your application significantly!
