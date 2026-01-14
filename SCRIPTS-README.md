# Solution Management Scripts

This directory contains PowerShell scripts to help you manage and reuse your solution structure.

## Scripts Overview

### 1. Rename-Solution.ps1
Renames all projects in your solution from one name to another while maintaining the same structure.

### 2. Restore-Packages.ps1
Restores all NuGet packages for your solution.

---

## Rename-Solution.ps1

### Purpose
This script allows you to create a template from your existing solution by renaming all projects. Perfect for reusing your solution structure for new projects.

### What It Does
1. Updates solution file (.sln) references
2. Renames project folders
3. Renames project files (.csproj)
4. Updates project references in .csproj files
5. Updates namespaces in all C# files
6. Restores NuGet packages
7. Cleans bin/obj folders
8. Creates a backup before making changes

### Usage Examples

**Basic usage:**
```powershell
.\Rename-Solution.ps1 -OldName "ESSPortal" -NewName "HRPortal"
```

**With custom solution name:**
```powershell
.\Rename-Solution.ps1 -OldName "ESSPortal" -NewName "HRPortal" -NewSolutionName "HR Management Portal"
```

**Specify solution path:**
```powershell
.\Rename-Solution.ps1 -OldName "ESSPortal" -NewName "InventorySystem" -SolutionPath "C:\Projects\NewProject"
```

**Without package restore:**
```powershell
.\Rename-Solution.ps1 -OldName "ESSPortal" -NewName "CRM" -RestorePackages $false
```

### Parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| OldName | Yes | - | The current project name prefix (e.g., "ESSPortal") |
| NewName | Yes | - | The new project name prefix (e.g., "HRPortal") |
| SolutionPath | No | Current directory | Path to the solution directory |
| NewSolutionName | No | - | New name for the .sln file (without extension) |
| RestorePackages | No | $true | Whether to restore packages after renaming |

### Before Running
1. **Close Visual Studio** - Make sure the solution is not open
2. **Commit your changes** - Have a clean git state or backup
3. **Review the old name** - Know exactly what you want to replace

### After Running
1. Review the changes in your source control
2. Build the solution: `dotnet build`
3. Run tests: `dotnet test`
4. If everything works, delete the backup folder

---

## Restore-Packages.ps1

### Purpose
Quickly restore all NuGet packages for your solution. Useful after cloning or switching branches.

### What It Does
1. Finds all projects in the solution
2. Optionally cleans bin/obj folders
3. Restores all NuGet packages
4. Optionally builds the solution
5. Shows a summary of all packages

### Usage Examples

**Basic usage:**
```powershell
.\Restore-Packages.ps1
```

**Restore and build:**
```powershell
.\Restore-Packages.ps1 -BuildAfterRestore $true
```

**Without cleaning:**
```powershell
.\Restore-Packages.ps1 -CleanFirst $false
```

**Specify solution path:**
```powershell
.\Restore-Packages.ps1 -SolutionPath "C:\Projects\MyProject"
```

### Parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| SolutionPath | No | Current directory | Path to the solution directory |
| CleanFirst | No | $true | Clean bin/obj before restoring |
| BuildAfterRestore | No | $false | Build the solution after restore |

---

## Common Workflows

### Creating a New Project from Template

1. **Copy your solution to a new location:**
   ```powershell
   Copy-Item -Path "D:\Repos\staff_portal" -Destination "D:\Repos\new_project" -Recurse
   cd D:\Repos\new_project
   ```

2. **Run the rename script:**
   ```powershell
   .\Rename-Solution.ps1 -OldName "ESSPortal" -NewName "NewProject" -NewSolutionName "New Project"
   ```

3. **Verify everything works:**
   ```powershell
   dotnet build
   ```

### After Cloning from Git

1. **Navigate to solution:**
   ```powershell
   cd D:\Repos\staff_portal
   ```

2. **Restore packages:**
   ```powershell
   .\Restore-Packages.ps1
   ```

3. **Build:**
   ```powershell
   dotnet build
   ```

### Switching Branches with Package Changes

1. **Switch branch:**
   ```powershell
   git checkout feature-branch
   ```

2. **Restore with clean:**
   ```powershell
   .\Restore-Packages.ps1 -CleanFirst $true
   ```

---

## Current Solution Structure

Your solution follows a clean architecture pattern:

```
ESS Portal/
├── src/
│   ├── Api/
│   │   └── ESSPortal.Api/
│   ├── Application/
│   │   └── ESSPortal.Application/
│   ├── Domain/
│   │   └── ESSPortal.Domain/
│   ├── Infrastructure/
│   │   └── ESSPortal.Infrastructure/
│   ├── Persistence/
│   │   └── ESSPortal.Persistence.SQLServer/
│   └── UI/
│       ├── ESSPortal.Web.Mvc/
│       └── ESSPortal.Web.Blazor/
└── ESS Portal.sln
```

When you rename from "ESSPortal" to a new name, this entire structure is preserved with all new names.

---

## Troubleshooting

### "Execution of scripts is disabled on this system"

Run this command in PowerShell as Administrator:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### "dotnet command not found"

Make sure .NET SDK is installed and in your PATH:
- Download from: https://dotnet.microsoft.com/download
- Verify with: `dotnet --version`

### Files are locked

Close Visual Studio and any other applications that might have files open.

### Package restore fails

1. Check your internet connection
2. Clear NuGet cache: `dotnet nuget locals all --clear`
3. Try restoring again

### Git shows too many changes

This is normal when renaming. Review carefully:
```powershell
git status
git diff
```

---

## Tips

1. **Always create a backup** - The rename script does this automatically
2. **Use descriptive names** - Choose clear, meaningful project names
3. **Test after renaming** - Always build and test after major changes
4. **Update documentation** - Remember to update README files with new names
5. **Clean regularly** - Run `Restore-Packages.ps1 -CleanFirst $true` to keep things tidy

---

## Package Information

Your solution currently uses these key packages:
- AutoMapper 16.0.0
- FluentValidation 12.1.1
- Microsoft.EntityFrameworkCore 10.0.1
- StackExchange.Redis 2.10.1
- jose-jwt 5.2.0
- MessagePack 3.1.4

All packages will be restored automatically when you run the scripts.
