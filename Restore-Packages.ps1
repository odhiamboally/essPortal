<#
.SYNOPSIS
    Restores NuGet packages for all projects in a solution

.DESCRIPTION
    This script restores all NuGet packages for a Visual Studio solution.
    It can also optionally clean bin/obj folders before restoring.

.PARAMETER SolutionPath
    Path to the solution directory. Defaults to current directory.

.PARAMETER CleanFirst
    Whether to clean bin/obj folders before restoring. Default is $true

.PARAMETER BuildAfterRestore
    Whether to build the solution after restoring packages. Default is $false

.EXAMPLE
    .\Restore-Packages.ps1

.EXAMPLE
    .\Restore-Packages.ps1 -SolutionPath "C:\Projects\MyProject" -BuildAfterRestore $true

.EXAMPLE
    .\Restore-Packages.ps1 -CleanFirst $false
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$SolutionPath = (Get-Location).Path,

    [Parameter(Mandatory=$false)]
    [bool]$CleanFirst = $true,

    [Parameter(Mandatory=$false)]
    [bool]$BuildAfterRestore = $false
)

$ErrorActionPreference = "Stop"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  NuGet Package Restore Tool" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Validate solution path
if (-not (Test-Path $SolutionPath)) {
    Write-Error "Solution path does not exist: $SolutionPath"
    exit 1
}

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
Write-Host "Solution: $($solutionFile.Name)" -ForegroundColor Yellow
Write-Host ""

# Find all project files
$projectFiles = Get-ChildItem -Path $SolutionPath -Filter "*.csproj" -Recurse -File
Write-Host "Found $($projectFiles.Count) project(s):" -ForegroundColor Green
foreach ($proj in $projectFiles) {
    Write-Host "  - $($proj.Name)" -ForegroundColor Gray
}
Write-Host ""

# Clean bin and obj folders if requested
if ($CleanFirst) {
    Write-Host "Cleaning build artifacts..." -ForegroundColor Cyan
    $cleanFolders = Get-ChildItem -Path $SolutionPath -Include "bin","obj" -Directory -Recurse

    if ($cleanFolders.Count -gt 0) {
        foreach ($folder in $cleanFolders) {
            Write-Host "  Removing: $($folder.FullName.Replace($SolutionPath, '.'))" -ForegroundColor Gray
            Remove-Item -Path $folder.FullName -Recurse -Force -ErrorAction SilentlyContinue
        }
        Write-Host "Clean completed" -ForegroundColor Green
    } else {
        Write-Host "No build artifacts to clean" -ForegroundColor Yellow
    }
    Write-Host ""
}

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Cyan

try {
    $dotnetVersion = dotnet --version
    Write-Host "Using .NET SDK version: $dotnetVersion" -ForegroundColor Green
    Write-Host ""

    Write-Host "Running: dotnet restore `"$($solutionFile.FullName)`"" -ForegroundColor Yellow
    dotnet restore $solutionFile.FullName --verbosity normal

    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "Package restore completed successfully!" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Warning "Package restore completed with errors. Exit code: $LASTEXITCODE"
        exit $LASTEXITCODE
    }
}
catch {
    Write-Error "Could not restore packages. Make sure .NET SDK is installed and in PATH."
    Write-Error "Error: $_"
    exit 1
}
Write-Host ""

# Build if requested
if ($BuildAfterRestore) {
    Write-Host "Building solution..." -ForegroundColor Cyan
    Write-Host "Running: dotnet build `"$($solutionFile.FullName)`"" -ForegroundColor Yellow
    dotnet build $solutionFile.FullName --no-restore

    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "Build completed successfully!" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Warning "Build completed with errors. Exit code: $LASTEXITCODE"
        exit $LASTEXITCODE
    }
    Write-Host ""
}

# Summary
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Package Restore Complete!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Package Information:" -ForegroundColor Yellow

foreach ($projectFile in $projectFiles) {
    [xml]$projectXml = Get-Content -Path $projectFile.FullName
    $packages = $projectXml.Project.ItemGroup.PackageReference

    if ($packages) {
        Write-Host ""
        Write-Host "  $($projectFile.Name):" -ForegroundColor White
        foreach ($package in $packages) {
            $packageName = $package.Include
            $version = $package.Version
            Write-Host "    - $packageName ($version)" -ForegroundColor Gray
        }
    }
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Green
