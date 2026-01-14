# Solution Template Usage Guide

## Overview

This guide shows you how to use your current solution as a **reusable template** to quickly create new projects without starting from scratch.

**Key Point:** Your original solution stays completely intact and can be used as a template indefinitely!

---

## Quick Start

### Creating a New Project from Template

```powershell
# Run this from your template directory or specify the template path
.\Create-From-Template.ps1 -OldName "ESSPortal" -NewName "HRPortal" -DestinationPath "D:\Repos\HR_Portal"
```

That's it! You now have a complete new solution at `D:\Repos\HR_Portal` with:
- All projects renamed to "HRPortal"
- All namespaces updated
- All project references updated
- Solution file renamed
- Packages restored and ready to build

**Your original template at the current location remains untouched!**

---

## The Scripts Explained

### Create-From-Template.ps1 ⭐ USE THIS FOR REUSING YOUR TEMPLATE

**What it does:**
1. **Copies** your entire solution to a new location
2. **Renames** everything in the copy (projects, namespaces, files)
3. **Restores** all NuGet packages
4. **Leaves your original template completely untouched**

**When to use:**
- Creating a new project based on your template
- You want to reuse your solution structure
- You want to keep the original for future use

**Example:**
```powershell
# Create a new HR Portal project
.\Create-From-Template.ps1 `
    -OldName "ESSPortal" `
    -NewName "HRPortal" `
    -DestinationPath "D:\Repos\HR_Portal"

# Create an Inventory System
.\Create-From-Template.ps1 `
    -OldName "ESSPortal" `
    -NewName "InventorySystem" `
    -DestinationPath "D:\Projects\Inventory" `
    -NewSolutionName "Inventory Management System"

# Use a different template location
.\Create-From-Template.ps1 `
    -OldName "ESSPortal" `
    -NewName "CRMSystem" `
    -TemplatePath "D:\Templates\ESSPortal" `
    -DestinationPath "D:\Repos\CRM"
```

### Rename-Solution.ps1 ⚠️ MODIFIES IN PLACE

**What it does:**
1. **Modifies** the current solution directly
2. Renames all projects, namespaces, and files
3. **Overwrites your original** - no going back!

**When to use:**
- One-time rename of an existing project
- You don't need to keep the original
- You're not using it as a template

**Example:**
```powershell
# Rename the current solution (destructive!)
.\Rename-Solution.ps1 -OldName "ESSPortal" -NewName "NewName"
```

### Restore-Packages.ps1 📦 PACKAGE MANAGEMENT

**What it does:**
- Restores all NuGet packages
- Optionally cleans bin/obj folders
- Optionally builds after restore

**When to use:**
- After creating a new project from template
- After cloning from git
- After switching branches
- When packages are out of sync

**Example:**
```powershell
# Simple restore
.\Restore-Packages.ps1

# Restore and build
.\Restore-Packages.ps1 -BuildAfterRestore $true

# Restore without cleaning
.\Restore-Packages.ps1 -CleanFirst $false
```

---

## Complete Workflow: Template to New Project

### Step 1: Keep Your Template Clean

Your current solution at `D:\Repos\Eclectics_2\staff_portal` is your template. Keep it:
- Clean and working
- Well-documented
- Free of sensitive data
- Free of project-specific code

### Step 2: Create a New Project

```powershell
# Navigate to your template directory
cd D:\Repos\Eclectics_2\staff_portal

# Create a new project
.\Create-From-Template.ps1 `
    -OldName "ESSPortal" `
    -NewName "HRPortal" `
    -DestinationPath "D:\Repos\HR_Portal"
```

### Step 3: Verify the New Project

```powershell
# Navigate to new project
cd D:\Repos\HR_Portal

# Build it
dotnet build

# Run it (example with API project)
dotnet run --project src\Api\HRPortal.Api
```

### Step 4: Initialize Git for New Project

```powershell
# In your new project directory
cd D:\Repos\HR_Portal

# Initialize git
git init

# Add all files
git add .

# First commit
git commit -m "Initial commit from ESSPortal template"

# Add remote (if you have one)
git remote add origin https://github.com/yourusername/hr-portal.git
git push -u origin main
```

### Step 5: Start Developing!

Your template remains at `D:\Repos\Eclectics_2\staff_portal` ready for the next project!

---

## Script Parameters Reference

### Create-From-Template.ps1

| Parameter | Required | Description | Example |
|-----------|----------|-------------|---------|
| **OldName** | ✅ Yes | Template project prefix | `"ESSPortal"` |
| **NewName** | ✅ Yes | New project prefix | `"HRPortal"` |
| **DestinationPath** | ✅ Yes | Where to create new solution | `"D:\Repos\HR_Portal"` |
| TemplatePath | No | Template location (default: current dir) | `"D:\Templates\ESS"` |
| NewSolutionName | No | Solution file name (default: NewName) | `"HR Management"` |
| RestorePackages | No | Restore packages after creation (default: true) | `$true` or `$false` |
| ExcludeFolders | No | Folders to skip when copying | `@(".git", "bin", "obj")` |

---

## Real-World Examples

### Example 1: Create HR Management System

```powershell
cd D:\Repos\Eclectics_2\staff_portal

.\Create-From-Template.ps1 `
    -OldName "ESSPortal" `
    -NewName "HRManagement" `
    -DestinationPath "D:\Repos\HR_Management" `
    -NewSolutionName "HR Management System"

# Result:
# - New solution at D:\Repos\HR_Management
# - All projects renamed: HRManagement.Api, HRManagement.Domain, etc.
# - Solution file: "HR Management System.sln"
# - Original template: Still intact at D:\Repos\Eclectics_2\staff_portal
```

### Example 2: Create Inventory System

```powershell
.\Create-From-Template.ps1 `
    -OldName "ESSPortal" `
    -NewName "InventorySystem" `
    -DestinationPath "C:\Projects\Inventory"

# Result:
# - New solution at C:\Projects\Inventory
# - All projects renamed: InventorySystem.Api, InventorySystem.Domain, etc.
# - Solution file: "InventorySystem.sln"
# - Ready to build and run
```

### Example 3: Create Customer Portal

```powershell
.\Create-From-Template.ps1 `
    -OldName "ESSPortal" `
    -NewName "CustomerPortal" `
    -DestinationPath "D:\Repos\Customer_Portal" `
    -RestorePackages $true

cd D:\Repos\Customer_Portal
dotnet build
dotnet run --project src\Api\CustomerPortal.Api
```

---

## What Gets Copied?

### ✅ Copied to New Location:
- All `.cs` source files
- All `.csproj` project files
- `.sln` solution file
- Configuration files (appsettings.json, etc.)
- Static files (wwwroot, assets, etc.)
- Documentation files

### ❌ NOT Copied (Excluded by Default):
- `.git` folder (git history)
- `.vs` folder (Visual Studio settings)
- `bin` folders (build output)
- `obj` folders (build artifacts)
- `packages` folder (NuGet packages - will be restored)
- `node_modules` folder (if any)
- Backup folders

**Why?** These are regenerated or user-specific, so excluding them keeps your new project clean.

---

## What Gets Renamed?

### In the New Solution:

1. **Solution File**
   - `ESS Portal.sln` → `HRPortal.sln` (or your custom name)

2. **Project Folders**
   - `src/Api/ESSPortal.Api` → `src/Api/HRPortal.Api`
   - `src/Application/ESSPortal.Application` → `src/Application/HRPortal.Application`
   - And so on for all 7 projects...

3. **Project Files**
   - `ESSPortal.Api.csproj` → `HRPortal.Api.csproj`
   - `ESSPortal.Domain.csproj` → `HRPortal.Domain.csproj`
   - Etc.

4. **Namespaces in C# Files**
   ```csharp
   // Before
   namespace ESSPortal.Domain.Entities
   using ESSPortal.Application.Services

   // After
   namespace HRPortal.Domain.Entities
   using HRPortal.Application.Services
   ```

5. **Project References**
   ```xml
   <!-- Before -->
   <ProjectReference Include="..\..\Domain\ESSPortal.Domain\ESSPortal.Domain.csproj" />

   <!-- After -->
   <ProjectReference Include="..\..\Domain\HRPortal.Domain\HRPortal.Domain.csproj" />
   ```

---

## Your Current Template Structure

```
ESS Portal/
├── src/
│   ├── Api/
│   │   └── ESSPortal.Api/                    → Becomes: YourName.Api/
│   ├── Application/
│   │   └── ESSPortal.Application/            → Becomes: YourName.Application/
│   ├── Domain/
│   │   └── ESSPortal.Domain/                 → Becomes: YourName.Domain/
│   ├── Infrastructure/
│   │   └── ESSPortal.Infrastructure/         → Becomes: YourName.Infrastructure/
│   ├── Persistence/
│   │   └── ESSPortal.Persistence.SQLServer/  → Becomes: YourName.Persistence.SQLServer/
│   └── UI/
│       ├── ESSPortal.Web.Mvc/                → Becomes: YourName.Web.Mvc/
│       └── ESSPortal.Web.Blazor/             → Becomes: YourName.Web.Blazor/
├── ESS Portal.sln                            → Becomes: YourName.sln
├── Create-From-Template.ps1                  → Your template creation script
├── Restore-Packages.ps1                      → Package restore script
└── TEMPLATE-USAGE-GUIDE.md                   → This file!
```

This clean architecture structure is preserved in every new project you create!

---

## Troubleshooting

### "Destination path already exists"

The script will ask if you want to overwrite. Options:
- Type `yes` to overwrite
- Type `no` to cancel and choose a different destination

### "No solution file found"

Make sure you're running from the correct directory or specify `-TemplatePath`:
```powershell
.\Create-From-Template.ps1 -TemplatePath "D:\Repos\Eclectics_2\staff_portal" ...
```

### "Execution of scripts is disabled"

Run PowerShell as Administrator and execute:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Packages fail to restore

1. Check internet connection
2. Clear NuGet cache: `dotnet nuget locals all --clear`
3. Try manual restore: `dotnet restore`

### Build fails after creation

1. Ensure all packages restored: `.\Restore-Packages.ps1`
2. Clean and rebuild: `dotnet clean && dotnet build`
3. Check for any template-specific code that needs updating

---

## Best Practices

### Maintaining Your Template

1. **Keep it generic** - Remove client-specific code
2. **Keep it clean** - No bin/obj/packages folders
3. **Keep it updated** - Update packages regularly
4. **Keep it documented** - Update README files
5. **Keep it working** - Ensure it builds successfully

### Creating New Projects

1. **Use descriptive names** - Choose clear project names
2. **Review before building** - Check renamed files look correct
3. **Test immediately** - Build and run right after creation
4. **Initialize git early** - Set up version control from the start
5. **Update configurations** - Change connection strings, API keys, etc.

### After Creation Checklist

- [ ] Build succeeds: `dotnet build`
- [ ] Tests pass (if any): `dotnet test`
- [ ] Update appsettings.json with project-specific settings
- [ ] Update README.md with new project information
- [ ] Initialize git repository
- [ ] Configure database connections
- [ ] Update any hardcoded values
- [ ] Set up CI/CD if needed

---

## Tips & Tricks

**Tip 1: Create a Templates Folder**
```powershell
# Keep dedicated templates
D:\Templates\
  ├── WebAPI_Template\
  ├── Blazor_Template\
  └── FullStack_Template\
```

**Tip 2: Use Consistent Naming**
```powershell
# Good naming convention
CompanyName.ProjectName.Layer

# Examples:
Acme.HR.Api
Acme.HR.Domain
Acme.Inventory.Application
```

**Tip 3: Script Your Common Setups**
```powershell
# Create and setup in one go
.\Create-From-Template.ps1 -OldName "ESSPortal" -NewName "MyProject" -DestinationPath "D:\Repos\MyProject"
cd D:\Repos\MyProject
git init
git add .
git commit -m "Initial commit"
code .  # Open in VS Code
```

**Tip 4: Keep Template Documentation**
Create a TEMPLATE-INFO.md in your template describing:
- What the template includes
- How to customize after creation
- Required environment setup
- Common configuration changes

---

## Summary

✅ **Your template stays intact** - Use it again and again
✅ **Fast project creation** - Minutes instead of hours
✅ **Consistent structure** - Same architecture every time
✅ **All packages restored** - Ready to build immediately
✅ **Clean separation** - Each project is independent

**Remember:** Use `Create-From-Template.ps1` to create new projects while keeping your template safe for future reuse!

---

## Need Help?

Check the companion scripts:
- **Create-From-Template.ps1** - This is your main tool for creating new projects
- **Restore-Packages.ps1** - Use this to restore packages anytime
- **Rename-Solution.ps1** - Only use for one-time in-place renames

Happy coding!
