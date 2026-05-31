# Scripts

> Hilfs- und Bootstrap-Skripte für lokale Entwicklung und einmalige Setup-Aufgaben.

**Owner:** `@TODO-org/communicationhub-devops`

| Skript | Zweck |
|--------|-------|
| [bootstrap-dev.ps1](bootstrap-dev.ps1) | Erst-Setup der lokalen Entwicklungsumgebung (Skelett). |

## Konventionen

- PowerShell 7+ (`pwsh`), kein Windows PowerShell 5.1.
- `Set-StrictMode -Version Latest`, `$ErrorActionPreference = 'Stop'`.
- Kein Hartcodieren von Subscription-/Tenant-IDs – via Parameter oder `az account` ableiten.
- Keine Secrets im Klartext; KV-Zugriff via `az keyvault secret show` mit interaktivem `az login`.
