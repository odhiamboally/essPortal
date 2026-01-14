<#
.SYNOPSIS
    Renames all projects in a solution and restores packages

.DESCRIPTION
    This script renames all projects in a Visual Studio solution by replacing the old project name prefix
    with a new one. It updates:
    - Project folder names
    - Project file names (.csproj)
    - Solution file references
    - Project references in .csproj files
    - Namespaces in .cs files
    - RootNamespace in .csproj files
    - AssemblyName in .csproj files

    After renaming, it restores all NuGet packages for the solution.

.PARAMETER OldName
    The old project name prefix to replace (e.g., "ESSPortal")

.PARAMETER NewName
    The new project name prefix (e.g., "MyNewProject")

.PARAMETER SolutionPath
    Path to the solution directory. Defaults to current directory.

.PARAMETER NewSolutionName
    Optional new name for the solution file (without .sln extension)

.PARAMETER RestorePackages
    Whether to restore NuGet packages after renaming. Default is $true

.EXAMPLE
    .\Rename-Solution.ps1 -OldName "ESSPortal" -NewName "HRPortal"

.EXAMPLE
    .\Rename-Solution.ps1 -OldName "ESSPortal" -NewName "HRPortal" -NewSolutionName "HR Portal" -SolutionPath "C:\Projects\MyProject"
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$OldName,

    [Parameter(Mandatory=$true)]
    [string]$NewName,

    [Parameter(Mandatory=$false)]
    [string]$SolutionPath = (Get-Location).Path,

    [Parameter(Mandatory=$false)]
    [string]$NewSolutionName = "",

    [Parameter(Mandatory=$false)]
    [bool]$RestorePackages = $true
)

# Set error action preference
$ErrorActionPreference = "Stop"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Solution Renaming and Package Restore Tool" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Validate solution path
if (-not (Test-Path $SolutionPath)) {
    Write-Error "Solution path does not exist: $SolutionPath"
    exit 1
}

# Change to solution directory
Set-Location $SolutionPath
Write-Host "Working directory: $SolutionPath" -ForegroundColor Green
Write-Host ""

# Find solution file
$solutionFiles = Get-ChildItem -Path $SolutionPath -Filter "*.sln" -File
if ($solutionFiles.Count -eq 0) {
    Write-Error "No solution file found in $SolutionPath"
    exit 1
}

if ($solutionFiles.Count -gt 1) {
    Write-Warning "Multiple solution files found. Using the first one: $($solutionFiles[0].Name)"
}

$solutionFile = $solutionFiles[0]
Write-Host "Solution file: $($solutionFile.Name)" -ForegroundColor Yellow
Write-Host ""

# Create backup
$backupFolder = Join-Path $SolutionPath "backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Write-Host "Creating backup in: $backupFolder" -ForegroundColor Yellow
New-Item -ItemType Directory -Path $backupFolder -Force | Out-Null
Copy-Item -Path $solutionFile.FullName -Destination $backupFolder
Write-Host "Backup created successfully" -ForegroundColor Green
Write-Host ""

# Step 1: Find all project files
Write-Host "Step 1: Finding all project files..." -ForegroundColor Cyan
$projectFiles = Get-ChildItem -Path $SolutionPath -Filter "*.csproj" -Recurse -File
Write-Host "Found $($projectFiles.Count) project(s)" -ForegroundColor Green
foreach ($proj in $projectFiles) {
    Write-Host "  - $($proj.FullName.Replace($SolutionPath, '.'))" -ForegroundColor Gray
}
Write-Host ""

# Step 2: Update solution file content
Write-Host "Step 2: Updating solution file..." -ForegroundColor Cyan
$solutionContent = Get-Content -Path $solutionFile.FullName -Raw -Encoding UTF8
$originalContent = $solutionContent

# Replace project names in solution file
$solutionContent = $solutionContent -replace [regex]::Escape($OldName), $NewName

if ($solutionContent -ne $originalContent) {
    Set-Content -Path $solutionFile.FullName -Value $solutionContent -Encoding UTF8 -NoNewline
    Write-Host "Solution file updated" -ForegroundColor Green
} else {
    Write-Host "No changes needed in solution file" -ForegroundColor Yellow
}
Write-Host ""

# Step 3: Update project files content (references, namespaces, assembly names)
Write-Host "Step 3: Updating project file contents..." -ForegroundColor Cyan
foreach ($projectFile in $projectFiles) {
    Write-Host "  Processing: $($projectFile.Name)" -ForegroundColor Yellow

    $content = Get-Content -Path $projectFile.FullName -Raw -Encoding UTF8
    $originalContent = $content

    # Replace project references
    $content = $content -replace [regex]::Escape($OldName), $NewName

    if ($content -ne $originalContent) {
        Set-Content -Path $projectFile.FullName -Value $content -Encoding UTF8 -NoNewline
        Write-Host "    Updated project references" -ForegroundColor Green
    }
}
Write-Host ""

# Step 4: Update namespaces in C# files
Write-Host "Step 4: Updating namespaces in C# files..." -ForegroundColor Cyan
$csFiles = Get-ChildItem -Path $SolutionPath -Filter "*.cs" -Recurse -File | Where-Object {
    $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\bin\*"
}
Write-Host "Found $($csFiles.Count) C# file(s)" -ForegroundColor Green

$updatedCount = 0
foreach ($csFile in $csFiles) {
    $content = Get-Content -Path $csFile.FullName -Raw -Encoding UTF8
    $originalContent = $content

    # Replace namespace declarations and using statements
    $content = $content -replace "namespace\s+$([regex]::Escape($OldName))", "namespace $NewName"
    $content = $content -replace "using\s+$([regex]::Escape($OldName))", "using $NewName"

    if ($content -ne $originalContent) {
        Set-Content -Path $csFile.FullName -Value $content -Encoding UTF8 -NoNewline
        $updatedCount++
    }
}
Write-Host "Updated $updatedCount C# file(s)" -ForegroundColor Green
Write-Host ""

# Step 5: Rename project folders and files
Write-Host "Step 5: Renaming project folders and files..." -ForegroundColor Cyan

# Get all directories that contain the old name (sorted by depth, deepest first)
$foldersToRename = Get-ChildItem -Path $SolutionPath -Directory -Recurse |
    Where-Object { $_.Name -like "*$OldName*" } |
    Sort-Object { $_.FullName.Split([IO.Path]::DirectorySeparatorChar).Count } -Descending

foreach ($folder in $foldersToRename) {
    $newFolderName = $folder.Name -replace [regex]::Escape($OldName), $NewName
    $newFolderPath = Join-Path $folder.Parent.FullName $newFolderName

    if ($folder.FullName -ne $newFolderPath) {
        Write-Host "  Renaming folder: $($folder.Name) -> $newFolderName" -ForegroundColor Yellow
        Rename-Item -Path $folder.FullName -NewName $newFolderName -Force
    }
}

# Rename project files
$projectFiles = Get-ChildItem -Path $SolutionPath -Filter "*$OldName*.csproj" -Recurse -File
foreach ($projectFile in $projectFiles) {
    $newFileName = $projectFile.Name -replace [regex]::Escape($OldName), $NewName
    $newFilePath = Join-Path $projectFile.Directory.FullName $newFileName

    if ($projectFile.FullName -ne $newFilePath) {
        Write-Host "  Renaming project file: $($projectFile.Name) -> $newFileName" -ForegroundColor Yellow
        Rename-Item -Path $projectFile.FullName -NewName $newFileName -Force
    }
}
Write-Host ""

# Step 6: Rename solution file if requested
if ($NewSolutionName -ne "") {
    Write-Host "Step 6: Renaming solution file..." -ForegroundColor Cyan
    $newSolutionFileName = "$NewSolutionName.sln"
    $newSolutionPath = Join-Path $SolutionPath $newSolutionFileName

    if ($solutionFile.FullName -ne $newSolutionPath) {
        Write-Host "  Renaming: $($solutionFile.Name) -> $newSolutionFileName" -ForegroundColor Yellow
        Rename-Item -Path $solutionFile.FullName -NewName $newSolutionFileName -Force
        $solutionFile = Get-Item $newSolutionPath
        Write-Host "Solution file renamed" -ForegroundColor Green
    }
    Write-Host ""
}

# Step 7: Restore NuGet packages
if ($RestorePackages) {
    Write-Host "Step 7: Restoring NuGet packages..." -ForegroundColor Cyan

    # Check if dotnet CLI is available
    try {
        $dotnetVersion = dotnet --version
        Write-Host "Using .NET SDK version: $dotnetVersion" -ForegroundColor Green

        Write-Host "Running: dotnet restore" -ForegroundColor Yellow
        dotnet restore $solutionFile.FullName

        if ($LASTEXITCODE -eq 0) {
            Write-Host "Package restore completed successfully" -ForegroundColor Green
        } else {
            Write-Warning "Package restore completed with errors. Exit code: $LASTEXITCODE"
        }
    }
    catch {
        Write-Warning "Could not restore packages. Make sure .NET SDK is installed and in PATH."
        Write-Warning "Error: $_"
    }
    Write-Host ""
}

# Step 8: Clean bin and obj folders
Write-Host "Step 8: Cleaning build artifacts..." -ForegroundColor Cyan
$cleanFolders = Get-ChildItem -Path $SolutionPath -Include "bin","obj" -Directory -Recurse
foreach ($folder in $cleanFolders) {
    Write-Host "  Removing: $($folder.FullName.Replace($SolutionPath, '.'))" -ForegroundColor Gray
    Remove-Item -Path $folder.FullName -Recurse -Force -ErrorAction SilentlyContinue
}
Write-Host "Build artifacts cleaned" -ForegroundColor Green
Write-Host ""

# Summary
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Renaming Complete!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Summary:" -ForegroundColor Yellow
Write-Host "  Old Name: $OldName" -ForegroundColor White
Write-Host "  New Name: $NewName" -ForegroundColor White
Write-Host "  Solution: $($solutionFile.Name)" -ForegroundColor White
Write-Host "  Backup Location: $backupFolder" -ForegroundColor White
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Review the changes" -ForegroundColor White
Write-Host "  2. Build the solution: dotnet build" -ForegroundColor White
Write-Host "  3. Run tests if applicable: dotnet test" -ForegroundColor White
Write-Host "  4. If everything works, you can delete the backup folder" -ForegroundColor White
Write-Host ""
Write-Host "Done!" -ForegroundColor Green
