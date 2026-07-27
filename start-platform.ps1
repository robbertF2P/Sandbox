<#
.SYNOPSIS
    Smart-start the V2 platform: rebuild any image whose source is newer than the image, then run the stack.
.PARAMETER Force
    Rebuild all images regardless of staleness.
.PARAMETER NoCache
    Pass --no-cache to podman build (use when a cached layer is known to be stale, e.g. after updating local-feed).
.EXAMPLE
    .\start-platform.ps1
    .\start-platform.ps1 -Force
    .\start-platform.ps1 -NoCache
#>
param(
    [switch]$Force,
    [switch]$NoCache
)

$ErrorActionPreference = "Continue"
Set-Location $PSScriptRoot

# ── helpers ────────────────────────────────────────────────────────────────────

function Get-ImageCreatedUtc([string]$tag) {
    $out = podman image inspect "localhost/$tag" --format "{{.Created}}" 2>$null
    if ($LASTEXITCODE -ne 0 -or -not $out) { return $null }
    # Podman: "2026-06-26 07:00:41.161430387 +0000 UTC" — trim nanos and tz name
    $normalized = $out.Trim() -replace '(\.\d{3})\d+', '$1' -replace '\s+[A-Z]{2,5}$', ''
    return [datetimeoffset]::Parse($normalized).UtcDateTime
}

function Get-NewestSourceUtc([string[]]$dirs, [string[]]$excludePrefixes = @()) {
    $skip = 'node_modules|dist|\.angular|\\bin\\|\\obj\\|TestResults'
    $max  = [datetime]::MinValue

    foreach ($dir in $dirs) {
        if (-not (Test-Path $dir)) { continue }
        Get-ChildItem $dir -Recurse -File -ErrorAction SilentlyContinue | ForEach-Object {
            $path = $_.FullName
            if ($path -match $skip) { return }
            foreach ($ex in $excludePrefixes) { if ($path.StartsWith($ex)) { return } }
            if ($_.LastWriteTimeUtc -gt $max) { $max = $_.LastWriteTimeUtc }
        }
    }
    if ($max -eq [datetime]::MinValue) { return $null }
    return $max
}

function Test-NeedsRebuild([string]$tag, [string[]]$sources, [string[]]$exclude = @()) {
    if ($Force) { Write-Host "  FORCE  $tag"; return $true }

    $created = Get-ImageCreatedUtc $tag
    if (-not $created) { Write-Host "  BUILD  $tag  (image not found)"; return $true }

    $newest = Get-NewestSourceUtc $sources $exclude
    if ($newest -and $newest -gt $created) {
        $delta = [math]::Round(($newest - $created).TotalMinutes, 0)
        Write-Host "  BUILD  $tag  (source is ${delta}m newer than image)"
        return $true
    }

    Write-Host "  OK     $tag"
    return $false
}

# ── image definitions ──────────────────────────────────────────────────────────

$f2pWebPath = $null
$abWebPath  = $null
if (Test-Path "F2pPlatform\web")      { $f2pWebPath = (Resolve-Path "F2pPlatform\web").Path }
if (Test-Path "AdminBackoffice\web")  { $abWebPath  = (Resolve-Path "AdminBackoffice\web").Path }

$images = @(
    @{
        Tag        = "floorganise-platform-f2p-platform-api:latest"
        Dockerfile = "F2pPlatform\host\F2pPlatform.Host\Dockerfile"
        Sources    = @("F2pPlatform", "Platform.Serilog.Logging", "Platform.ControlPlane.Contracts", "local-feed", "build")
        Exclude    = @($f2pWebPath)
    }
    @{
        Tag        = "floorganise-platform-admin-backoffice-api:latest"
        Dockerfile = "AdminBackoffice\host\AdminBackoffice.Host\Dockerfile"
        Sources    = @("AdminBackoffice", "Platform.Serilog.Logging", "Platform.ControlPlane.Contracts", "local-feed", "build")
        Exclude    = @($abWebPath)
    }
    @{
        Tag        = "floorganise-platform-f2p-platform-web:latest"
        Dockerfile = "F2pPlatform\web\Dockerfile"
        Sources    = @("F2pPlatform\web", "FloorganiseCss")
        Exclude    = @()
    }
    @{
        Tag        = "floorganise-platform-admin-backoffice-web:latest"
        Dockerfile = "AdminBackoffice\web\Dockerfile"
        Sources    = @("AdminBackoffice\web", "FloorganiseCss")
        Exclude    = @()
    }
)

# ── staleness check ────────────────────────────────────────────────────────────

Write-Host "`nChecking images..."
$toBuild = $images | Where-Object { Test-NeedsRebuild $_.Tag $_.Sources $_.Exclude }

# ── build stale images ─────────────────────────────────────────────────────────

if ($toBuild) {
    $buildFlags = @()
    if ($NoCache) { $buildFlags = @("--no-cache") }

    Write-Host "`nBringing stack down before rebuild..."
    podman compose -f docker-compose.platform.yml down *>$null

    foreach ($img in $toBuild) {
        Write-Host "`nBuilding $($img.Tag)..."
        podman build @buildFlags -f $img.Dockerfile -t $img.Tag .
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Build failed for $($img.Tag). Fix the error and re-run."
            exit 1
        }
    }
}

# ── start the stack ────────────────────────────────────────────────────────────

Write-Host "`nStarting stack..."
podman compose -f docker-compose.platform.yml down *>$null
podman compose -f docker-compose.platform.yml up -d

Write-Host @"

Stack is up:
  F2P UI        http://localhost:5180
  Admin UI      http://localhost:5190
  F2P Swagger   http://localhost:5080/swagger
  Admin Swagger http://localhost:5090/swagger
  Seq logs      http://localhost:8080
"@
