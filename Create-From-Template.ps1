<#
.SYNOPSIS
    Creates a new solution from a template by copying and renaming all projects

.DESCRIPTION
    This script creates a NEW solution in a different location by copying your template solution
    and renaming all projects. The original template remains completely untouched.

    It performs these operations on the NEW copy:
    - Copies entire solution to new location
    - Renames project folder names
    - Renames project file names (.csproj)
    - Updates solution file references
    - Updates project references in .csproj files
    - Updates namespaces in .cs files
    - Restores NuGet packages
    - Cleans bin/obj folders

.PARAMETER OldName
    The template project name prefix (e.g., "ESSPortal")

.PARAMETER NewName
    The new project name prefix (e.g., "HRPortal")

.PARAMETER DestinationPath
    Path where the new solution will be created (e.g., "D:\Repos\HR_Portal")

.PARAMETER TemplatePath
    Path to the template solution directory. Defaults to current directory.

.PARAMETER NewSolutionName
    Optional new name for the solution file (without .sln extension). If not provided, uses NewName.

.PARAMETER RestorePackages
    Whether to restore NuGet packages after creating. Default is $true

.PARAMETER ExcludeFolders
    Folders to exclude when copying. Default excludes .git, .vs, bin, obj, etc.

.EXAMPLE
    .\Create-From-Template.ps1 -OldName "ESSPortal" -NewName "HRPortal" -DestinationPath "D:\Repos\HR_Portal"

.EXAMPLE
    .\Create-From-Template.ps1 -OldName "ESSPortal" -NewName "HRPortal" -DestinationPath "D:\Repos\HR_Portal" -NewSolutionName "HR Management Portal"

.EXAMPLE
    .\Create-From-Template.ps1 -OldName "ESSPortal" -NewName "InventorySystem" -TemplatePath "D:\Templates\ESSPortal" -DestinationPath "D:\Projects\Inventory"
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$OldName,

    [Parameter(Mandatory=$true)]
    [string]$NewName,

    [Parameter(Mandatory=$true)]
    [string]$DestinationPath,

    [Parameter(Mandatory=$false)]
    [string]$TemplatePath = (Get-Location).Path,

    [Parameter(Mandatory=$false)]
    [string]$NewSolutionName = "",

    [Parameter(Mandatory=$false)]
    [bool]$RestorePackages = $true,

    [Parameter(Mandatory=$false)]
    [string[]]$ExcludeFolders = @(".git", ".vs", "bin", "obj", "packages", "node_modules", ".idea", "backup_*", ".vscode")
)

# Set error action preference
$ErrorActionPreference = "Stop"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Solution Template Creator" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Validate template path
if (-not (Test-Path $TemplatePath)) {
    Write-Error "Template path does not exist: $TemplatePath"
    exit 1
}

# Check if destination already exists
if (Test-Path $DestinationPath) {
    Write-Warning "Destination path already exists: $DestinationPath"
    $response = Read-Host "Do you want to overwrite it? (yes/no)"
    if ($response -ne "yes") {
        Write-Host "Operation cancelled by user" -ForegroundColor Yellow
        exit 0
    }
    Write-Host "Removing existing destination..." -ForegroundColor Yellow
    Remove-Item -Path $DestinationPath -Recurse -Force
}

Write-Host "Template: $TemplatePath" -ForegroundColor Green
Write-Host "Destination: $DestinationPath" -ForegroundColor Green
Write-Host ""

# Find solution file in template
$templateSolutionFiles = Get-ChildItem -Path $TemplatePath -Filter "*.sln" -File
if ($templateSolutionFiles.Count -eq 0) {
    Write-Error "No solution file found in template path: $TemplatePath"
    exit 1
}

if ($templateSolutionFiles.Count -gt 1) {
    Write-Warning "Multiple solution files found in template. Using the first one: $($templateSolutionFiles[0].Name)"
}

$templateSolutionFile = $templateSolutionFiles[0]
Write-Host "Template solution: $($templateSolutionFile.Name)" -ForegroundColor Yellow
Write-Host ""

# Step 1: Copy template to destination
Write-Host "Step 1: Copying template to destination..." -ForegroundColor Cyan
Write-Host "This may take a moment..." -ForegroundColor Gray

# Create destination parent directory if it doesn't exist
$destinationParent = Split-Path -Parent $DestinationPath
if ($destinationParent -and -not (Test-Path $destinationParent)) {
    New-Item -ItemType Directory -Path $destinationParent -Force | Out-Null
}

# Copy with exclusions
function Copy-WithExclusions {
    param(
        [string]$Source,
        [string]$Destination,
        [string[]]$Exclude
    )

    # Create destination directory
    New-Item -ItemType Directory -Path $Destination -Force | Out-Null

    # Get all items in source
    $items = Get-ChildItem -Path $Source -Force

    foreach ($item in $items) {
        $shouldExclude = $false

        # Check if item matches any exclusion pattern
        foreach ($pattern in $Exclude) {
            if ($item.Name -like $pattern) {
                $shouldExclude = $true
                Write-Host "  Skipping: $($item.Name)" -ForegroundColor DarkGray
                break
            }
        }

        if (-not $shouldExclude) {
            $destPath = Join-Path $Destination $item.Name

            if ($item.PSIsContainer) {
                # Recursively copy directory
                Copy-WithExclusions -Source $item.FullName -Destination $destPath -Exclude $Exclude
            } else {
                # Copy file
                Copy-Item -Path $item.FullName -Destination $destPath -Force
            }
        }
    }
}

Copy-WithExclusions -Source $TemplatePath -Destination $DestinationPath -Exclude $ExcludeFolders

Write-Host "Copy completed successfully" -ForegroundColor Green
Write-Host ""

# Change to destination directory for remaining operations
Set-Location $DestinationPath
Write-Host "Working directory: $DestinationPath" -ForegroundColor Green
Write-Host ""

# Find solution file in destination
$solutionFiles = Get-ChildItem -Path $DestinationPath -Filter "*.sln" -File
$solutionFile = $solutionFiles[0]

# Step 2: Find all project files
Write-Host "Step 2: Finding all project files in new location..." -ForegroundColor Cyan
$projectFiles = Get-ChildItem -Path $DestinationPath -Filter "*.csproj" -Recurse -File
Write-Host "Found $($projectFiles.Count) project(s)" -ForegroundColor Green
foreach ($proj in $projectFiles) {
    Write-Host "  - $($proj.FullName.Replace($DestinationPath, '.'))" -ForegroundColor Gray
}
Write-Host ""

# Step 3: Update solution file content
Write-Host "Step 3: Updating solution file..." -ForegroundColor Cyan
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

# Step 4: Update project files content (references, namespaces, assembly names)
Write-Host "Step 4: Updating project file contents..." -ForegroundColor Cyan
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

# Step 5: Update namespaces in C# files
Write-Host "Step 5: Updating namespaces in C# files..." -ForegroundColor Cyan
$csFiles = Get-ChildItem -Path $DestinationPath -Filter "*.cs" -Recurse -File | Where-Object {
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

# Step 6: Rename project folders and files
Write-Host "Step 6: Renaming project folders and files..." -ForegroundColor Cyan

# Get all directories that contain the old name (sorted by depth, deepest first)
$foldersToRename = Get-ChildItem -Path $DestinationPath -Directory -Recurse |
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
$projectFiles = Get-ChildItem -Path $DestinationPath -Filter "*$OldName*.csproj" -Recurse -File
foreach ($projectFile in $projectFiles) {
    $newFileName = $projectFile.Name -replace [regex]::Escape($OldName), $NewName
    $newFilePath = Join-Path $projectFile.Directory.FullName $newFileName

    if ($projectFile.FullName -ne $newFilePath) {
        Write-Host "  Renaming project file: $($projectFile.Name) -> $newFileName" -ForegroundColor Yellow
        Rename-Item -Path $projectFile.FullName -NewName $newFileName -Force
    }
}
Write-Host ""

# Step 7: Rename solution file if requested
if ($NewSolutionName -ne "") {
    Write-Host "Step 7: Renaming solution file..." -ForegroundColor Cyan
    $newSolutionFileName = "$NewSolutionName.sln"
    $newSolutionPath = Join-Path $DestinationPath $newSolutionFileName

    if ($solutionFile.FullName -ne $newSolutionPath) {
        Write-Host "  Renaming: $($solutionFile.Name) -> $newSolutionFileName" -ForegroundColor Yellow
        Rename-Item -Path $solutionFile.FullName -NewName $newSolutionFileName -Force
        $solutionFile = Get-Item $newSolutionPath
        Write-Host "Solution file renamed" -ForegroundColor Green
    }
    Write-Host ""
} else {
    # Use NewName as solution name
    Write-Host "Step 7: Renaming solution file to match project name..." -ForegroundColor Cyan
    $newSolutionFileName = "$NewName.sln"
    $newSolutionPath = Join-Path $DestinationPath $newSolutionFileName

    if ($solutionFile.FullName -ne $newSolutionPath) {
        Write-Host "  Renaming: $($solutionFile.Name) -> $newSolutionFileName" -ForegroundColor Yellow
        Rename-Item -Path $solutionFile.FullName -NewName $newSolutionFileName -Force
        $solutionFile = Get-Item $newSolutionPath
        Write-Host "Solution file renamed" -ForegroundColor Green
    }
    Write-Host ""
}

# Step 8: Clean bin and obj folders
Write-Host "Step 8: Cleaning build artifacts..." -ForegroundColor Cyan
$cleanFolders = Get-ChildItem -Path $DestinationPath -Include "bin","obj" -Directory -Recurse
foreach ($folder in $cleanFolders) {
    Write-Host "  Removing: $($folder.FullName.Replace($DestinationPath, '.'))" -ForegroundColor Gray
    Remove-Item -Path $folder.FullName -Recurse -Force -ErrorAction SilentlyContinue
}
Write-Host "Build artifacts cleaned" -ForegroundColor Green
Write-Host ""

# Step 9: Restore NuGet packages
if ($RestorePackages) {
    Write-Host "Step 9: Restoring NuGet packages..." -ForegroundColor Cyan

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

# Summary
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  New Solution Created Successfully!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Summary:" -ForegroundColor Yellow
Write-Host "  Template: $TemplatePath" -ForegroundColor White
Write-Host "  New Location: $DestinationPath" -ForegroundColor White
Write-Host "  Old Name: $OldName" -ForegroundColor White
Write-Host "  New Name: $NewName" -ForegroundColor White
Write-Host "  Solution: $($solutionFile.Name)" -ForegroundColor White
Write-Host ""
Write-Host "Your original template at:" -ForegroundColor Yellow
Write-Host "  $TemplatePath" -ForegroundColor White
Write-Host "Remains completely untouched and ready for reuse!" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Navigate to new solution: cd '$DestinationPath'" -ForegroundColor White
Write-Host "  2. Build the solution: dotnet build" -ForegroundColor White
Write-Host "  3. Run the solution: dotnet run --project src\Api\$NewName.Api" -ForegroundColor White
Write-Host "  4. Initialize git if needed: git init" -ForegroundColor White
Write-Host ""
Write-Host "Done!" -ForegroundColor Green
