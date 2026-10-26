# Checks the installed twblazor.templates NuGet tool before creating an app.
#
# With no arguments it only checks for an update. Pass the same arguments you
# would give to `twblazor new` to check first and then create the app:
#
#   ./scripts/pre-create-template.ps1
#   ./scripts/pre-create-template.ps1 MyApp -t blank
#
# If an explicit template source is used (--templates), the NuGet tool check is skipped.

$ErrorActionPreference = 'Stop'

# Windows PowerShell 5.1 otherwise negotiates an older TLS version with NuGet.
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$packageId = 'twblazor.templates'
$packageUrl = 'https://www.nuget.org/packages/twblazor.templates'
$arguments = @($args)

function Has-ExplicitTemplateSource {
    for ($index = 0; $index -lt $arguments.Count; $index++) {
        if ($arguments[$index] -eq '--templates' -or $arguments[$index] -like '--templates=*') {
            return $true
        }
    }

    return $false
}

function Get-InstalledTemplateTool {
    $json = & dotnet tool list --global --format json 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($json)) {
        return $null
    }

    $tools = $json | ConvertFrom-Json
    return @($tools.data) | Where-Object { $_.packageId -eq $packageId } | Select-Object -First 1
}

function Get-LatestTemplateVersion {
    $index = Invoke-RestMethod 'https://api.nuget.org/v3-flatcontainer/twblazor.templates/index.json'
    $versions = @($index.versions | Where-Object { $_ -notmatch '-' })
    if ($versions.Count -eq 0) {
        return $null
    }

    return $versions | Sort-Object { [version]$_ } | Select-Object -Last 1
}

function Get-BaseVersion([string]$version) {
    return [version]($version -replace '[-+].*$', '')
}

function Update-TemplateTool([string]$version) {
    if (Get-InstalledTemplateTool) {
        & dotnet tool update --global $packageId --version $version
    }
    else {
        & dotnet tool install --global $packageId --version $version
    }

    if ($LASTEXITCODE -ne 0) {
        throw "Could not install $packageId $version."
    }
}

$explicitSource = Has-ExplicitTemplateSource

if ($explicitSource) {
    Write-Host 'Using an explicit template source; skipping the NuGet template update check.' -ForegroundColor DarkGray
}
else {
    try {
        $latestVersion = Get-LatestTemplateVersion
    }
    catch {
        Write-Warning "Could not check NuGet for the latest $packageId version: $($_.Exception.Message)"
        $latestVersion = $null
    }

    if ($latestVersion) {
        $installed = Get-InstalledTemplateTool

        if (-not $installed) {
            Write-Host "$packageId is not installed. The latest NuGet version is $latestVersion." -ForegroundColor Yellow
            Write-Host "NuGet: $packageUrl" -ForegroundColor DarkGray
            $answer = Read-Host 'Install it now? [y/N]'

            if ($answer -match '^(y|yes)$') {
                Update-TemplateTool $latestVersion
                Write-Host "Installed $packageId $latestVersion." -ForegroundColor Green
            }
            else {
                Write-Host "Cannot create the app without $packageId. Install it with 'dotnet tool install --global $packageId'." -ForegroundColor Yellow
                Write-Host "NuGet: $packageUrl" -ForegroundColor DarkGray
                exit 1
            }
        }
        else {
            $installedVersion = $installed.version
            $needsUpdate = (Get-BaseVersion $installedVersion) -lt (Get-BaseVersion $latestVersion) -or
                ((Get-BaseVersion $installedVersion) -eq (Get-BaseVersion $latestVersion) -and $installedVersion -ne $latestVersion)

            if ($needsUpdate) {
                Write-Host "$packageId $installedVersion is installed, but $latestVersion is available on NuGet." -ForegroundColor Yellow
                Write-Host "NuGet: $packageUrl" -ForegroundColor DarkGray
                $answer = Read-Host 'Update before creating the app? [y/N]'

                if ($answer -match '^(y|yes)$') {
                    Update-TemplateTool $latestVersion
                    Write-Host "Updated $packageId to $latestVersion." -ForegroundColor Green
                }
                else {
                    Write-Host "Continuing with $packageId $installedVersion." -ForegroundColor DarkGray
                }
            }
            else {
                Write-Host "$packageId $installedVersion is up to date." -ForegroundColor Green
            }
        }
    }
}

if ($arguments.Count -eq 0) {
    Write-Host 'Ready. Create an app with: twblazor new MyApp' -ForegroundColor Green
    exit 0
}

if (-not (Get-Command twblazor -ErrorAction SilentlyContinue)) {
    throw "The 'twblazor' command was not found. Install it with 'dotnet tool install --global $packageId' and restart your terminal."
}

& twblazor new @arguments
exit $LASTEXITCODE
