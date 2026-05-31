# Contributing

> Detaillierte Konventionen: [docs/plan/19-repo-scaffolding.md](docs/plan/19-repo-scaffolding.md)

## Branching

- **Trunk-based**: `main` ist deploybar.
- Kurzlebige Feature-Branches: `feat/<kurz>`, `fix/<kurz>`, `chore/<kurz>`, `docs/<kurz>`.
- Keine direkten Pushes auf `main` – nur via Pull Request.
- Branch-Lebensdauer Ziel: **< 3 Tage** vor Merge.

## Pull Requests

- Mindestens **1 Reviewer** (für sicherheitsrelevante Pfade, AL-Datenmodell, Bicep: **2 Reviewer**).
- Alle CI-Checks müssen grün sein (Build, Lint, Test, CodeQL, Dependency-Review).
- PR-Template (siehe [.github/pull_request_template.md](.github/pull_request_template.md)) ausfüllen.
- Konflikte vor Merge auflösen, kein `--no-verify`, kein `--force-with-lease` auf `main`.
- Verlinkung auf Plandokument(e) und/oder Issue erwünscht.

## Commit-Nachrichten – Conventional Commits

```
<type>(<scope>): <kurzbeschreibung>

[optional body]
[optional footer(s)]
```

Typen: `feat`, `fix`, `chore`, `docs`, `refactor`, `perf`, `test`, `build`, `ci`, `style`, `revert`.

Beispiele:

- `feat(bc-extension): add Communication Interaction table skeleton`
- `ci(backend): add dotnet test step`
- `docs(plan): clarify graph subscription renewal`

## Code-Owner

Siehe [CODEOWNERS](CODEOWNERS). Reviews werden automatisch angefordert.

## Sicherheit

- **Keine Secrets im Repo** (siehe `.gitignore`).
- CI/CD nutzt **OIDC Federated Identity** gegen Azure – keine Service-Principal-Secrets als GitHub Secrets.
- Bei sicherheitsrelevanten Findings: [SECURITY.md](SECURITY.md).

## Tests

- Komponententests laufen mit jeder PR.
- E2E-/AI-Eval-Suiten siehe [docs/plan/16-testing-acceptance.md](docs/plan/16-testing-acceptance.md).

## TODOs in dieser Sprint-0-Phase

- Reviewer-Mindestanzahl in `main`-Branch-Protection setzen.
- CODEOWNERS-Teams definieren.
- Conventional-Commits-Linter (z. B. `commitlint`) aktivieren.
