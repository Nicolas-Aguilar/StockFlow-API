param(
    [switch]$RunApi
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptRoot
$apiProject = Join-Path $repoRoot "src/StockFlow.Api/StockFlow.Api.csproj"
$infrastructureProject = Join-Path $repoRoot "src/StockFlow.Infrastructure/StockFlow.Infrastructure.csproj"
$solutionPath = Join-Path $repoRoot "StockFlow.sln"
$envPath = Join-Path $repoRoot ".env"
$composePath = Join-Path $repoRoot "docker-compose.yml"

function Assert-CommandAvailable {
    param([string]$CommandName)

    if (-not (Get-Command $CommandName -ErrorAction SilentlyContinue)) {
        throw "Required command '$CommandName' was not found in PATH."
    }
}

function Invoke-ExternalCommand {
    param(
        [string]$FilePath,
        [string[]]$Arguments
    )

    & $FilePath @Arguments | Out-Host

    if ($LASTEXITCODE -ne 0) {
        throw "Command failed: $FilePath $($Arguments -join ' ')"
    }
}

function New-SecretString {
    param([int]$ByteCount = 32)

    $bytes = New-Object byte[] $ByteCount
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($bytes)
    $rng.Dispose()
    return [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', 'A').Replace('/', 'B')
}

function Read-KeyValueFile {
    param([string]$Path)

    $values = @{}

    if (-not (Test-Path $Path)) {
        return $values
    }

    foreach ($line in Get-Content -Path $Path) {
        if ([string]::IsNullOrWhiteSpace($line) -or $line.TrimStart().StartsWith("#")) {
            continue
        }

        $parts = $line -split '=', 2
        if ($parts.Length -eq 2) {
            $values[$parts[0].Trim()] = $parts[1].Trim()
        }
    }

    return $values
}

function Write-KeyValueFile {
    param(
        [string]$Path,
        [hashtable]$Values
    )

    $content = @(
        "SQLSERVER_SA_PASSWORD=$($Values["SQLSERVER_SA_PASSWORD"])",
        "SQLSERVER_SQL_PORT=$($Values["SQLSERVER_SQL_PORT"])")

    Set-Content -Path $Path -Value $content -Encoding ascii
}

function Get-UserSecretsMap {
    param([string]$Project)

    $values = @{}
    $lines = & dotnet user-secrets list --project $Project 2>$null

    foreach ($line in $lines) {
        $parts = $line -split ' = ', 2
        if ($parts.Length -eq 2) {
            $values[$parts[0].Trim()] = $parts[1].Trim()
        }
    }

    return $values
}

function Set-UserSecretIfDifferent {
    param(
        [string]$Project,
        [hashtable]$CurrentSecrets,
        [string]$Key,
        [string]$Value
    )

    if ($CurrentSecrets[$Key] -ne $Value) {
        & dotnet user-secrets set $Key $Value --project $Project | Out-Null
        $CurrentSecrets[$Key] = $Value
    }
}

function Wait-ForSqlServer {
    param([int]$Attempts = 40)

    for ($attempt = 1; $attempt -le $Attempts; $attempt++) {
        $status = & docker inspect --format "{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}" stockflow-sqlserver 2>$null

        if ($status -eq "healthy") {
            return
        }

        Start-Sleep -Seconds 3
    }

    throw "SQL Server container did not become healthy in time. Check 'docker compose logs sqlserver'."
}

Assert-CommandAvailable dotnet
Assert-CommandAvailable docker

if (-not (Test-Path $composePath)) {
    throw "docker-compose.yml was not found at '$composePath'."
}

$envValues = Read-KeyValueFile -Path $envPath
$resetSqlVolume = $false

if ([string]::IsNullOrWhiteSpace($envValues["SQLSERVER_SA_PASSWORD"]) -or $envValues["SQLSERVER_SA_PASSWORD"] -like "__*") {
    $envValues["SQLSERVER_SA_PASSWORD"] = "LocalDev_$(New-SecretString -ByteCount 24)!aA1"
    $resetSqlVolume = $true
}

if ([string]::IsNullOrWhiteSpace($envValues["SQLSERVER_SQL_PORT"])) {
    $envValues["SQLSERVER_SQL_PORT"] = "1433"
}

Write-KeyValueFile -Path $envPath -Values $envValues

$connectionString = "Server=localhost,$($envValues["SQLSERVER_SQL_PORT"]);Database=StockFlowDb;User Id=sa;Password=$($envValues["SQLSERVER_SA_PASSWORD"]);TrustServerCertificate=True;"
$currentSecrets = Get-UserSecretsMap -Project $apiProject

Set-UserSecretIfDifferent -Project $apiProject -CurrentSecrets $currentSecrets -Key "ConnectionStrings:DefaultConnection" -Value $connectionString

if ([string]::IsNullOrWhiteSpace($currentSecrets["Jwt:Key"])) {
    Set-UserSecretIfDifferent -Project $apiProject -CurrentSecrets $currentSecrets -Key "Jwt:Key" -Value ("LocalJwt_" + (New-SecretString -ByteCount 48))
}

Write-Host "[1/5] Starting SQL Server container..."
if ($resetSqlVolume) {
    Write-Host "Detected new local SQL credentials. Recreating SQL Server volume to keep credentials in sync..."
    Invoke-ExternalCommand -FilePath "docker" -Arguments @("compose", "--file", $composePath, "--project-directory", $repoRoot, "down", "-v", "--remove-orphans")
}

Invoke-ExternalCommand -FilePath "docker" -Arguments @("compose", "--file", $composePath, "--project-directory", $repoRoot, "up", "-d", "sqlserver")

Write-Host "[2/5] Waiting for SQL Server healthcheck..."
Wait-ForSqlServer

Write-Host "[3/5] Restoring tools and packages..."
Invoke-ExternalCommand -FilePath "dotnet" -Arguments @("tool", "restore", "--tool-manifest", (Join-Path $repoRoot ".config/dotnet-tools.json"))
Invoke-ExternalCommand -FilePath "dotnet" -Arguments @("restore", $solutionPath)

Write-Host "[4/5] Applying migrations..."
Invoke-ExternalCommand -FilePath "dotnet" -Arguments @("ef", "database", "update", "--project", $infrastructureProject, "--startup-project", $apiProject)

Write-Host "[5/5] Building solution..."
Invoke-ExternalCommand -FilePath "dotnet" -Arguments @("build", $solutionPath, "--no-restore")

Write-Host "StockFlow local environment is ready."
Write-Host "SQL Server: localhost:$($envValues["SQLSERVER_SQL_PORT"])"
Write-Host "Swagger: http://localhost:5133/swagger"

if ($RunApi) {
    Write-Host "Starting API..."
    Invoke-ExternalCommand -FilePath "dotnet" -Arguments @("run", "--project", $apiProject, "--no-build")
}
