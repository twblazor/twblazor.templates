[CmdletBinding()]
param(
    [ValidateSet("server", "wasm", "auto")]
    [string]$Interactivity = "server",

    [ValidateSet("dashboard", "sidebar", "navbar")]
    [string]$Layout = "dashboard",

    [switch]$NoRestore,
    [switch]$NoNpm,
    [switch]$NoRun,
    [switch]$Open
)

$ErrorActionPreference = "Stop"

$repoRoot = $PSScriptRoot
$cliProject = Join-Path $repoRoot "src/TwBlazor.Templates.Cli/TwBlazor.Templates.Cli.csproj"
$templates = Join-Path $repoRoot "templates/twblazor"

if (-not (Test-Path $cliProject)) {
    throw "The CLI project was not found at '$cliProject'."
}

if (-not (Test-Path $templates)) {
    throw "The template folder was not found at '$templates'."
}

$name = "Preview" + (Get-Culture).TextInfo.ToTitleCase($Interactivity) + (Get-Culture).TextInfo.ToTitleCase($Layout)
$output = Join-Path $repoRoot "artifacts/preview/$name"
$appProject = Join-Path $output $name
$solution = Join-Path $output "$name.slnx"

Write-Host "Generating $name in $output..." -ForegroundColor Cyan

$arguments = @(
    "run", "--project", $cliProject, "--",
    "new", $name,
    "-i", $Interactivity,
    "-t", $Layout,
    "-o", $output,
    "--templates", $templates,
    "--force"
)

if ($NoRestore) {
    $arguments += "--no-restore"
}

if ($NoNpm) {
    $arguments += "--no-npm"
}

& dotnet @arguments

if ($LASTEXITCODE -ne 0) {
    throw "The preview app could not be generated."
}

if ($Open) {
    Start-Process $solution
    return
}

if ($NoRun) {
    Write-Host "Generate only. Run it with:" -ForegroundColor Green
    Write-Host "  dotnet run --project `"$appProject`" --launch-profile http"
    return
}

$url = $null
$launchSettings = Join-Path $appProject "Properties/launchSettings.json"

if (Test-Path $launchSettings) {
    $settings = Get-Content $launchSettings -Raw | ConvertFrom-Json
    $url = $settings.profiles.http.applicationUrl
}

if ($url) {
    Write-Host "Preview: $url" -ForegroundColor Green
}

Write-Host "Press Ctrl+C to stop." -ForegroundColor DarkGray

Push-Location $output

try {
    & dotnet run --project $appProject --launch-profile http
}
finally {
    Pop-Location
}
