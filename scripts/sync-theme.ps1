[CmdletBinding()]
param(
    [string]$Ref = 'main',
    [string]$Source,
    [string]$Target,
    [switch]$Check
)

$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path $PSScriptRoot -Parent
if (-not $Target) {
    $Target = Join-Path $repositoryRoot 'templates/twblazor/ui/Theme.cs'
}

function Read-ThemeSource {
    param([string]$Location)

    if ([Uri]::IsWellFormedUriString($Location, [UriKind]::Absolute)) {
        return (Invoke-WebRequest -Uri $Location -UseBasicParsing).Content
    }

    if (Test-Path -LiteralPath $Location) {
        return [IO.File]::ReadAllText((Resolve-Path -LiteralPath $Location))
    }

    throw "Theme source '$Location' is neither a URL nor a file."
}

$content = $null
if ($Source) {
    $content = Read-ThemeSource $Source
    $sourceDescription = $Source
}
else {
    $candidates = @(
        'src/Theme/Theme.cs',
        'src/TwBlazor.Theme/Theme.cs'
    )
    $errors = [System.Collections.Generic.List[string]]::new()

    foreach ($path in $candidates) {
        $url = "https://raw.githubusercontent.com/twblazor/twblazor/$Ref/$path"
        try {
            $content = Read-ThemeSource $url
            $sourceDescription = $url
            break
        }
        catch {
            $errors.Add("$url ($($_.Exception.Message))")
        }
    }

    if (-not $content) {
        throw "Could not download the twblazor theme from '$Ref'. Tried:`n$($errors -join "`n")"
    }
}

# The canonical file is the working default theme used by the theme builder. The template copy
# uses the app's namespace, so it can be edited by the app, and omits the live-reload handler.
$content = $content -replace '(?m)^using System\.Reflection\.Metadata;\r?\n', ''
$content = $content -replace '(?m)^\[assembly: MetadataUpdateHandler\([^\r\n]*\)\]\r?\n', ''
$content = $content -replace '(?m)^namespace TwBlazor\.Theme;\r?\n', "#if (HasClient)`nnamespace TwBlazorApp.Client;`n#else`nnamespace TwBlazorApp;`n#endif`n"
$content = $content -replace '(?m)^using TwBlazor\.Enums;\r?\n\r?\n\r?\n(?=#if \(HasClient\))', "using TwBlazor.Enums;`n`n"

$content = [regex]::Replace(
    $content,
    '(?s)\r?\ninternal static class ThemeUpdateHandler\s*\{.*?\r?\n\}\s*$',
    '')

$templateHeader = @'
/// <summary>
/// The classes every twblazor component is styled with. Tailwind builds the app's stylesheet from this file.
/// </summary>
/// <remarks>
/// This is the default twblazor theme. To change colours, rounding, shadows or the components included,
/// generate a new theme at https://twblazor.com/theme and replace this whole file.
/// </remarks>
'@

$content = $content.Replace(
    'public static class Theme',
    "$templateHeader`npublic static class Theme")

$content = ($content -replace "`r`n", "`n").TrimEnd("`n") + "`n"

if ($content -notmatch '#if \(HasClient\)' -or $content -notmatch 'namespace TwBlazorApp;' -or $content -notmatch 'public static TwBlazorTheme CreateDefaultTheme') {
    throw "The transformed theme does not look like a template Theme.cs. The canonical theme format may have changed."
}

$targetPath = [IO.Path]::GetFullPath($Target)
$existing = if (Test-Path -LiteralPath $targetPath) {
    [IO.File]::ReadAllText($targetPath)
}
else {
    $null
}

if ($Check) {
    if ($existing -ne $content) {
        throw "Theme.cs is out of date with $sourceDescription. Run scripts/sync-theme.ps1 to update it."
    }

    Write-Host "Theme.cs matches $sourceDescription."
    return
}

if ($existing -eq $content) {
    Write-Host "Theme.cs is already up to date with $sourceDescription."
    return
}

$targetDirectory = Split-Path $targetPath -Parent
New-Item -ItemType Directory -Force -Path $targetDirectory | Out-Null
[IO.File]::WriteAllText($targetPath, $content, [Text.UTF8Encoding]::new($false))
Write-Host "Updated $targetPath from $sourceDescription."
