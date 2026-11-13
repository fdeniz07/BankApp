# Quick daily lab smoke (run from repo root or infra/).
# Usage: pwsh -File infra/scripts/lab-daily-check.ps1
$ErrorActionPreference = 'Continue'
$repoRoot = (Get-Item $PSScriptRoot).Parent.Parent.FullName
$infraDir = Join-Path $repoRoot 'infra'
if (-not (Test-Path (Join-Path $infraDir '.env'))) {
    Write-Warning 'Missing infra/.env — copy from .env.example'
}
Set-Location $infraDir

function Get-EnvDefault {
    param([string]$Name, [string]$Default)
    $path = Join-Path $infraDir '.env'
    if (-not (Test-Path $path)) { return $Default }
    $line = Get-Content $path | Where-Object { $_ -match "^$Name=" } | Select-Object -First 1
    if ($line -match "^$Name=(.+)$") { return $Matches[1].Trim() }
    return $Default
}

$webapiPort = [int](Get-EnvDefault 'WEBAPI_PORT' '8080')
$esPort = [int](Get-EnvDefault 'ELASTICSEARCH_PORT' '9200')
$redisPassword = Get-EnvDefault 'REDIS_PASSWORD' 'change-me-redis'

Write-Host '=== docker compose ps (core) ===' -ForegroundColor Cyan
docker compose --env-file .env ps vault sqlserver redis elasticsearch webapi client 2>&1

Write-Host "`n=== WebApi /health ===" -ForegroundColor Cyan
try {
    $r = Invoke-WebRequest -Uri "http://localhost:$webapiPort/health" -UseBasicParsing -TimeoutSec 5
    Write-Host "HTTP $($r.StatusCode)" -ForegroundColor Green
} catch {
    Write-Warning "Health check failed: $_"
}

Write-Host "`n=== Redis PING (requirepass) ===" -ForegroundColor Cyan
$redisPing = docker compose --env-file .env exec -T redis redis-cli -a $redisPassword ping 2>&1
if ($redisPing -match 'PONG') {
    Write-Host 'PONG' -ForegroundColor Green
} else {
    Write-Warning "Redis auth ping failed: $redisPing"
}

Write-Host "`n=== Vault overlay (last lines) ===" -ForegroundColor Cyan
docker compose --env-file .env logs webapi --tail 80 2>&1 | Select-String -Pattern 'Vault overlay|FailClosed|VaultFailClosed' | Select-Object -Last 5

Write-Host "`n=== Vault AppRole policy deny (observability) ===" -ForegroundColor Cyan
try {
    $roleId = (docker compose --env-file .env exec -T webapi cat /vault-approle/role_id 2>&1).ToString().Trim()
    $secretId = (docker compose --env-file .env exec -T webapi cat /vault-approle/secret_id 2>&1).ToString().Trim()
    if (-not $roleId -or -not $secretId) {
        Write-Warning 'AppRole files missing on WebApi — run vault-seed and recreate webapi'
    } else {
        $loginJson = docker compose --env-file .env exec -T vault vault write -format=json auth/approle/login "role_id=$roleId" "secret_id=$secretId" 2>&1
        $login = $loginJson | ConvertFrom-Json
        $clientToken = $login.auth.client_token
        $obsOut = docker compose --env-file .env exec -T -e "VAULT_TOKEN=$clientToken" vault vault kv get secret/bankapp/observability 2>&1
        if ($LASTEXITCODE -ne 0 -or ($obsOut -match 'permission denied|403|not authorized')) {
            Write-Host 'AppRole cannot read observability (expected)' -ForegroundColor Green
        } else {
            Write-Warning "AppRole read observability unexpectedly succeeded: $obsOut"
        }
    }
} catch {
    Write-Warning "Vault policy smoke failed: $_"
}

Write-Host "`n=== Elasticsearch log indices (bankapp-logs*) ===" -ForegroundColor Cyan
try {
    $idx = curl.exe -s "http://localhost:${esPort}/_cat/indices/bankapp-logs*?v" 2>&1
    if ($idx -and $idx -notmatch 'index_not_found|error') {
        Write-Host $idx
    } else {
        Write-Warning 'No bankapp-logs* indices yet — send traffic or wait for Logstash (optional for cold stack)'
    }
} catch {
    Write-Warning "ES index check failed: $_"
}

Write-Host "`n=== dotnet test (integration smoke) ===" -ForegroundColor Cyan
Set-Location $repoRoot
dotnet test tests/WebApi.IntegrationTests/WebApi.IntegrationTests.csproj -v q --nologo 2>&1 | Select-Object -Last 4

Write-Host "`nDone. If Vault was recreated: docker compose --env-file infra/.env up vault-seed" -ForegroundColor DarkGray
Write-Host "Then: docker compose --env-file infra/.env up -d --force-recreate --no-deps webapi" -ForegroundColor DarkGray
