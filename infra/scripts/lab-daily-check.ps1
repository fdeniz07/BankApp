# Quick daily lab smoke (run from repo root or infra/).
# Usage: pwsh -File infra/scripts/lab-daily-check.ps1
$ErrorActionPreference = 'Continue'
$repoRoot = (Get-Item $PSScriptRoot).Parent.Parent.FullName
if (-not (Test-Path "$repoRoot\infra\.env")) {
    Write-Warning 'Missing infra/.env — copy from .env.example'
}
Set-Location "$repoRoot\infra"

Write-Host '=== docker compose ps (core) ===' -ForegroundColor Cyan
docker compose --env-file .env ps vault sqlserver redis webapi client 2>&1

$webapiPort = 8080
if (Test-Path .env) {
    $line = Get-Content .env | Where-Object { $_ -match '^WEBAPI_PORT=' } | Select-Object -First 1
    if ($line -match '^WEBAPI_PORT=(\d+)') { $webapiPort = [int]$Matches[1] }
}

Write-Host "`n=== WebApi /health ===" -ForegroundColor Cyan
try {
    $r = Invoke-WebRequest -Uri "http://localhost:$webapiPort/health" -UseBasicParsing -TimeoutSec 5
    Write-Host "HTTP $($r.StatusCode)" -ForegroundColor Green
} catch {
    Write-Warning "Health check failed: $_"
}

Write-Host "`n=== Vault overlay (last lines) ===" -ForegroundColor Cyan
docker compose --env-file .env logs webapi --tail 80 2>&1 | Select-String -Pattern 'Vault overlay|FailClosed|VaultFailClosed' | Select-Object -Last 5

Write-Host "`n=== dotnet test (integration smoke) ===" -ForegroundColor Cyan
Set-Location $repoRoot
dotnet test tests/WebApi.IntegrationTests/WebApi.IntegrationTests.csproj -v q --nologo 2>&1 | Select-Object -Last 4

Write-Host "`nDone. If Vault was recreated: docker compose --env-file infra/.env up vault-seed" -ForegroundColor DarkGray
Write-Host "Then: docker compose --env-file infra/.env up -d --force-recreate --no-deps webapi" -ForegroundColor DarkGray
