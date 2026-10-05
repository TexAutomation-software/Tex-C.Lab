param(
    [switch]$LaunchInstaller
)

$ErrorActionPreference = "Stop"

# ------------------------------------------------------------
# Repository paths
# ------------------------------------------------------------

$Root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

$ProjectFile = Join-Path $Root "P-CRA.csproj"

$ManualTex = Join-Path $Root "Docs/UserManual.tex"
$ChangelogTex = Join-Path $Root "Changelog/Changelog.tex"

$InnoScript = Join-Path $PSScriptRoot "Setup-script.iss"

# $PdfOutput = Join-Path $Root "build"
$InstallerOutput = Join-Path $Root "Setup/Output"

$Runtime = "win-x64"
$Configuration = "Release"

# ------------------------------------------------------------
# Required tools
# ------------------------------------------------------------

$IsccCandidates = @(
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe"
)

$Iscc = $IsccCandidates |
    Where-Object { Test-Path $_ } |
    Select-Object -First 1

if (-not $Iscc) {
    throw "Could not find ISCC.exe. Install Inno Setup 6 or update the script path."
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "dotnet was not found in PATH."
}

if (-not (Get-Command pdflatex -ErrorAction SilentlyContinue)) {
    throw "pdflatex was not found in PATH. Install LaTeX in WSL or Windows and expose it to PowerShell."
}

# ------------------------------------------------------------
# Helper functions
# ------------------------------------------------------------

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)]
        [string]$File,

        [Parameter(Mandatory = $false)]
        [string[]]$Arguments = @()
    )

    Write-Host ""
    Write-Host ">>> $File $($Arguments -join ' ')" -ForegroundColor Cyan

    & $File @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw "Command failed with exit code $LASTEXITCODE`: $File"
    }
}

function Update-LatexMetadata {
    param(
        [Parameter(Mandatory = $true)]
        [string]$File,

        [Parameter(Mandatory = $true)]
        [string]$Version,

        [Parameter(Mandatory = $true)]
        [string]$BuildDate
    )

    if (-not (Test-Path $File)) {
        throw "LaTeX file not found: $File"
    }

    $Content = Get-Content -Path $File -Raw

    $VersionLine = "\newcommand{\DocVersion}{$Version}"
    $BuildDateLine = "\newcommand{\BuildDate}{$BuildDate}"

    $VersionPattern = '(?m)^\s*\\newcommand\{\\DocVersion\}\{[^}]*\}\s*$'
    $BuildDatePattern = '(?m)^\s*\\newcommand\{\\BuildDate\}\{[^}]*\}\s*$'

    if ($Content -notmatch $VersionPattern) {
        throw "Could not find \DocVersion in $File"
    }

    if ($Content -notmatch $BuildDatePattern) {
        throw "Could not find \BuildDate in $File"
    }

    $Content = [regex]::Replace(
        $Content,
        $VersionPattern,
        $VersionLine
    )

    $Content = [regex]::Replace(
        $Content,
        $BuildDatePattern,
        $BuildDateLine
    )

    Set-Content -Path $File -Value $Content -Encoding UTF8

    Write-Host "Updated LaTeX metadata: $File" -ForegroundColor Green
}

# ------------------------------------------------------------
# Read Version and BuildDate from the .csproj
# ------------------------------------------------------------

if (-not (Test-Path $ProjectFile)) {
    throw "Project file not found: $ProjectFile"
}

[xml]$ProjectXml = Get-Content -Path $ProjectFile -Raw

$PropertyGroup = $ProjectXml.Project.PropertyGroup |
    Where-Object {
        $_.Version -and $_.BuildDate
    } |
    Select-Object -First 1

if (-not $PropertyGroup) {
    throw "Could not find both Version and BuildDate in $ProjectFile"
}

$Version = [string]$PropertyGroup.Version
$BuildDate = [string]$PropertyGroup.BuildDate

if ([string]::IsNullOrWhiteSpace($Version)) {
    throw "Version is empty in $ProjectFile"
}

if ([string]::IsNullOrWhiteSpace($BuildDate)) {
    throw "BuildDate is empty in $ProjectFile"
}

if ($Version -notmatch '^\d+\.\d+\.\d+') {
    throw "Version '$Version' is not in the expected format, for example 0.0.1"
}

if ($BuildDate -notmatch '^\d{8}$') {
    throw "BuildDate '$BuildDate' is not in the expected format YYYYMMDD, for example 20261005"
}

$VersionForFilename = $Version -replace '\.', '-'

Write-Host ""
Write-Host "Release metadata" -ForegroundColor Yellow
Write-Host "  Version:          $Version"
Write-Host "  Build date:       $BuildDate"
Write-Host "  Filename version: $VersionForFilename"

# ------------------------------------------------------------
# Prepare output directories
# ------------------------------------------------------------

# New-Item -ItemType Directory -Force -Path $PdfOutput | Out-Null
New-Item -ItemType Directory -Force -Path $InstallerOutput | Out-Null

# Remove old PDFs so stale files cannot be mistaken for new output.
# Remove-Item -Force -ErrorAction SilentlyContinue `
    # (Join-Path $PdfOutput "UserManual.pdf")

# Remove-Item -Force -ErrorAction SilentlyContinue `
    # (Join-Path $PdfOutput "changelog.pdf")

# ------------------------------------------------------------
# Inject metadata into LaTeX files
# ------------------------------------------------------------

Update-LatexMetadata `
    -File $ManualTex `
    -Version $Version `
    -BuildDate $BuildDate

Update-LatexMetadata `
    -File $ChangelogTex `
    -Version $Version `
    -BuildDate $BuildDate

# ------------------------------------------------------------
# Compile UserManual.tex
# ------------------------------------------------------------

$ManualDirectory = Split-Path $ManualTex
$ManualFilename = Split-Path $ManualTex -Leaf

$ManualBuildDir = Join-Path $ManualDirectory "build"

New-Item -ItemType Directory -Force -Path $ManualBuildDir | Out-Null

Push-Location $ManualDirectory

try {
    Invoke-Checked "pdflatex" @(
        "-interaction=nonstopmode",
        "-halt-on-error",
        "-output-directory=build",
        $ManualFilename
    )

    Invoke-Checked "pdflatex" @(
        "-interaction=nonstopmode",
        "-halt-on-error",
        "-output-directory=build",
        $ManualFilename
    )
}
finally {
    Pop-Location
}

# ------------------------------------------------------------
# Compile changelog.tex
# ------------------------------------------------------------

$ChangelogDirectory = Split-Path $ChangelogTex
$ChangelogFilename = Split-Path $ChangelogTex -Leaf

$ChangelogBuildDir = Join-Path $ChangelogDirectory "build"

New-Item -ItemType Directory -Force -Path $ChangelogBuildDir | Out-Null

Push-Location $ChangelogDirectory

try {
    Invoke-Checked "pdflatex" @(
        "-interaction=nonstopmode",
        "-halt-on-error",
        "-output-directory=build",
        $ChangelogFilename
    )

    Invoke-Checked "pdflatex" @(
        "-interaction=nonstopmode",
        "-halt-on-error",
        "-output-directory=build",
        $ChangelogFilename
    )
}
finally {
    Pop-Location
}

# ------------------------------------------------------------
# Publish the Windows application
# ------------------------------------------------------------

$PublishDirectory = Join-Path $Root "bin/Release/net9.0/$Runtime/publish"

if (Test-Path $PublishDirectory) {
    Remove-Item -Recurse -Force $PublishDirectory
}

Invoke-Checked "dotnet" @(
    "publish",
    $ProjectFile,
    "-c", $Configuration,
    "-r", $Runtime,
    "--self-contained", "true",
    "-p:Version=$Version",
    "-p:BuildDate=$BuildDate",
    "-o", $PublishDirectory
)

# ------------------------------------------------------------
# Build the Inno Setup installer
# ------------------------------------------------------------

Invoke-Checked $Iscc @(
    "/DMyAppVersion=$Version",
    "/DMyAppVersionFile=$VersionForFilename",
    "/DBuildDate=$BuildDate",
    "/O$InstallerOutput",
    $InnoScript
)

$InstallerPath = Join-Path `
    $InstallerOutput `
    "TexCLab-installer-$VersionForFilename.exe"

if (-not (Test-Path $InstallerPath)) {
    throw "Inno Setup completed, but installer was not found at: $InstallerPath"
}

Write-Host ""
Write-Host "Release completed successfully." -ForegroundColor Green
Write-Host ""
Write-Host "User Manual:      $ManualBuildDir\UserManual.pdf"
Write-Host "Changelog:        $ChangelogBuildDir\Changelog.pdf"
Write-Host "Publish output:   $PublishDirectory"
Write-Host "Installer:        $InstallerPath"

# ------------------------------------------------------------
# Optionally launch the installer
# ------------------------------------------------------------

if ($LaunchInstaller) {
    Write-Host ""
    Write-Host "Launching installer..." -ForegroundColor Yellow
    Start-Process -FilePath $InstallerPath
}
