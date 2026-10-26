[CmdletBinding()]
param(
    [string]$BaseUrl = 'http://localhost:5300',
    [string]$OutputDirectory = 'images/templates'
)

$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path $PSScriptRoot -Parent
$outputPath = Join-Path $repositoryRoot $OutputDirectory
New-Item -ItemType Directory -Force -Path $outputPath | Out-Null

$scriptPath = Join-Path ([IO.Path]::GetTempPath()) 'twblazor-templates-screenshots.js'

@'
const { chromium } = require('playwright');
const path = require('path');

const base = process.env.TEMPLATES_URL;
const out = path.resolve(process.env.TEMPLATES_OUTPUT);
const routes = [
  ['dashboard', '/'],
  ['sidebar', '/sidebar'],
  ['navbar', '/navbar'],
  ['blank', '/blank'],
];

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 1440, height: 1050 }, colorScheme: 'light' });

  for (const [name, route] of routes) {
    await page.goto(base + route, { waitUntil: 'domcontentloaded' });
    await page.waitForSelector('h1', { timeout: 10000 });
    await page.waitForTimeout(1800);

    // The preview switcher is not part of a template.
    await page.evaluate(() => document.querySelector('div[title="Preview layout"]')?.remove());

    await page.screenshot({ path: path.join(out, `${name}.png`) });
    console.log(`Captured ${name}.png`);
  }

  await browser.close();
})();
'@ | Set-Content -LiteralPath $scriptPath -Encoding utf8NoBOM

$env:TEMPLATES_URL = $BaseUrl
$env:TEMPLATES_OUTPUT = $outputPath

Push-Location $repositoryRoot
try {
    npx --yes --package playwright node $scriptPath
}
finally {
    Pop-Location
    Remove-Item -LiteralPath $scriptPath -Force -ErrorAction SilentlyContinue
    Remove-Item Env:\TEMPLATES_URL -ErrorAction SilentlyContinue
    Remove-Item Env:\TEMPLATES_OUTPUT -ErrorAction SilentlyContinue
}
