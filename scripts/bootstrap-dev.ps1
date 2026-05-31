#requires -Version 7.0
<#
.SYNOPSIS
    Bootstrap-Skript für die lokale Entwicklungsumgebung des Customer Communication Copilots.

.DESCRIPTION
    Skelett (MVP1 Sprint 0). Prüft Werkzeug-Versionen, führt az login durch und
    bereitet die dev-Subscription vor. Funktionalität folgt in Sprint 1.

.PARAMETER SubscriptionId
    Zielsubscription für lokale Tests (dev).

.PARAMETER TenantId
    Entra-ID-Tenant. Wenn leer, wird der aktuell angemeldete Tenant verwendet.

.EXAMPLE
    pwsh ./scripts/bootstrap-dev.ps1 -SubscriptionId 00000000-0000-0000-0000-000000000000
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string] $SubscriptionId,

    [Parameter(Mandatory = $false)]
    [string] $TenantId,

    [Parameter(Mandatory = $false)]
    [ValidateSet('dev', 'test', 'prod')]
    [string] $Env = 'dev'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host '=== Customer Communication Copilot – Bootstrap (Skelett) ===' -ForegroundColor Cyan
Write-Host "Env=$Env  Subscription=$SubscriptionId  Tenant=$TenantId"
Write-Host ''

# TODO Sprint 1: Tool-Checks
# - dotnet --list-sdks  (>= 8.0)
# - node --version      (>= 20)
# - pnpm --version      (>= 9)   (optional)
# - az version          (Bicep eingebaut)
# - git --version
Write-Host '[TODO Sprint 1] Werkzeug-Versionen prüfen (dotnet, node, pnpm, az, git).'

# TODO Sprint 1: az login (Device Code, falls headless)
# if (-not (az account show 2>$null)) { az login --tenant $TenantId }
Write-Host '[TODO Sprint 1] az login (interaktiv, kein Service-Principal-Secret).'

# TODO Sprint 1: az account set --subscription $SubscriptionId
Write-Host '[TODO Sprint 1] az account set / az bicep upgrade.'

# TODO Sprint 1: Initial Bicep what-if gegen dev
# az deployment sub what-if `
#   --location swedencentral `
#   --template-file infra/bicep/main.bicep `
#   --parameters infra/bicep/parameters/$Env.bicepparam
Write-Host "[TODO Sprint 1] Bicep what-if gegen infra/bicep/parameters/$Env.bicepparam."

# TODO Sprint 1: dotnet restore / build der Backend-Solution
Write-Host '[TODO Sprint 1] dotnet restore + build (src/backend).'

# TODO Sprint 1: pnpm install in src/outlook-addin und src/teams-app
Write-Host '[TODO Sprint 1] pnpm install (src/outlook-addin, src/teams-app).'

Write-Host ''
Write-Host 'Fertig (Skelett). Siehe docs/plan/19-repo-scaffolding.md für die nächsten Schritte.' -ForegroundColor Green
