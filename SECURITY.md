# Security Policy

> Konzeptionelle Grundlagen: [docs/plan/12-security-compliance.md](docs/plan/12-security-compliance.md)

## Sicherheits-Issues melden

**Bitte keine sicherheitsrelevanten Findings als öffentliche GitHub-Issues anlegen.**

Meldungen bitte vertraulich per E-Mail an:

```
security@iointegrated.com
```

Verantwortliches Team: [@IOIntegrated/communicationhub-security](https://github.com/orgs/IOIntegrated/teams/communicationhub-security).

> Bei besonders sensiblen Findings (z. B. aktive Datenabfluss-Risiken) bitte zusätzlich ein Mitglied des Teams direkt kontaktieren. Eine PGP-Verschlüsselungsoption wird in Sprint 1 ergänzt.

Bitte enthalten:

- Betroffene Komponente (`src/bc-extension`, `src/backend`, `infra`, …)
- Reproduktionsschritte / Proof of Concept
- Vermutete Auswirkung (Vertraulichkeit / Integrität / Verfügbarkeit)
- Optional: vorgeschlagener Fix

## Antwortzeiten (Ziel)

| Schweregrad | Initial-Antwort | Fix-Ziel |
|-------------|-----------------|----------|
| Kritisch    | 24 h            | 7 Tage   |
| Hoch        | 48 h            | 30 Tage  |
| Mittel      | 5 Werktage      | Nächstes Minor-Release |
| Niedrig     | 10 Werktage     | Nach Priorisierung |

## Scope

- Code in diesem Repository (alle Komponenten).
- Bereitgestellte Bicep-Templates unter `infra/`.
- CI/CD-Workflows unter `.github/workflows/`.

Out of Scope: Microsoft 365 / Azure / Business Central Plattform-Sicherheitslücken – bitte direkt an Microsoft melden (MSRC).

## Sicherheits-Defaults dieses Repos

- **Keine Secrets im Repo** (`.gitignore` blockt `secrets.json`, `*.pfx`, `.env*`, `**/secrets/**`).
- CI/CD nutzt **OIDC Federated Identity** gegen Azure (siehe [docs/plan/19-repo-scaffolding.md §5](docs/plan/19-repo-scaffolding.md)).
- **CodeQL** und **Dependency Review** sind als Workflows angelegt.
- Branch-Protection / Required Reviews sind in der GitHub-Org-Konfiguration zu aktivieren (TODO Sprint 0).
