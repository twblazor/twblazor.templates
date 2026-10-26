<#
.SYNOPSIS
    Updates the template Theme.cs and twblazor package references.

.DESCRIPTION
    Version is the twblazor package version written to the template projects. Ref is the git ref the
    canonical Theme.cs is read from. A stable package defaults to its release tag (v<Version>); a
    pre-release package defaults to develop, because preview packages are not tagged.

.EXAMPLE
    ./update-twblazor.ps1 -Version 3.1.0
    Uses the canonical theme from tag v3.1.0.

.EXAMPLE
    ./update-twblazor.ps1 -Version 3.0.1-preview.2
    Uses the canonical theme from develop and the 3.0.1-preview.2 GitHub package.

.EXAMPLE
    ./update-twblazor.ps1 -Version 3.0.1-preview.2 -Source ../twblazor/src/TwBlazor.Theme/Theme.cs
    Uses a local twblazor checkout instead of downloading the theme.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory, Position = 0)]
    [string]$Version,

    [string]$Ref,
    [string]$Source
)

$ErrorActionPreference = 'Stop'

$Version = $Version.Trim()
if ($Version.StartsWith('v', [StringComparison]::OrdinalIgnoreCase)) {
    $Version = $Version.Substring(1)
}

if (-not $Version) {
    throw 'A twblazor version is required.'
}

if ($Version -match '^(develop|main)$') {
    throw "'$Version' is a branch, not a package version. For a preview package from develop, use -Version 3.0.1-preview.2 -Ref develop."
}

if ($Version -notmatch '^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$') {
    throw "'$Version' is not a package version. Use a version such as 3.1.0 or 3.0.1-preview.2."
}

if (-not $Ref) {
    $Ref = if ($Version.Contains('-')) { 'develop' } else { "v$Version" }
}

$repositoryRoot = Split-Path $PSScriptRoot -Parent

$syncArguments = @{ Ref = $Ref }
if ($Source) {
    $syncArguments.Source = $Source
}

& (Join-Path $PSScriptRoot 'sync-theme.ps1') @syncArguments

$expectedProjects = @(
    'templates/twblazor/host/TwBlazorApp.csproj',
    'templates/twblazor/client/TwBlazorApp.Client.csproj',
    'preview/TwBlazor.Templates.Preview/TwBlazor.Templates.Preview.csproj'
)

$packagePattern = '(<PackageReference\b[^>]*\bInclude="twblazor"[^>]*\bVersion=")[^"]+(")'

foreach ($relativePath in $expectedProjects) {
    $path = Join-Path $repositoryRoot $relativePath
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Expected project '$relativePath' was not found."
    }

    $project = [IO.File]::ReadAllText($path)
    if ([regex]::Matches($project, $packagePattern).Count -eq 0) {
        throw "No twblazor PackageReference was found in '$relativePath'."
    }

    $updated = [regex]::Replace($project, $packagePattern, "`${1}$Version`${2}", 1)

    if ($updated -eq $project) {
        Write-Host "$relativePath already references twblazor $Version."
        continue
    }

    [IO.File]::WriteAllText($path, $updated, [Text.UTF8Encoding]::new($false))
    Write-Host "Updated $relativePath to twblazor $Version."
}

$readmePath = Join-Path $repositoryRoot 'templates/twblazor/README.md'
if (-not (Test-Path -LiteralPath $readmePath)) {
    throw 'templates/twblazor/README.md was not found.'
}

$readme = [IO.File]::ReadAllText($readmePath)
if ([regex]::Matches($readme, '(`twblazor`\s+)\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?(\s+NuGet package)').Count -eq 0) {
    throw 'The twblazor version in templates/twblazor/README.md was not found.'
}

$updatedReadme = [regex]::Replace(
    $readme,
    '(`twblazor`\s+)\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?(\s+NuGet package)',
    "`${1}$Version`${2}",
    1)

if ($updatedReadme -eq $readme) {
    Write-Host "templates/twblazor/README.md already references twblazor $Version."
    return
}

[IO.File]::WriteAllText($readmePath, $updatedReadme, [Text.UTF8Encoding]::new($false))
Write-Host "Updated templates/twblazor/README.md to twblazor $Version."
