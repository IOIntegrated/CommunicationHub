# Outlook Add-in

> Office.js / React Side-Panel für Outlook (Desktop/Web/Mobile). Liefert Kontext, Quellen und Antwortentwürfe zum aktuellen E-Mail-Item – **kein Auto-Send**.

**Plandokumente:**
- [docs/plan/04-outlook-addin.md](../../docs/plan/04-outlook-addin.md) – Funktionen, UX, Compose-Insert
- [docs/plan/12-security-compliance.md §2.2](../../docs/plan/12-security-compliance.md) – SSO / OBO

**Owner:** `@TODO-org/communicationhub-frontend`

**Stack:** TypeScript, React, Office.js, Vite (oder Webpack via Yo Office). Auth via `Office.context.auth.getAccessToken` (SSO) → Backend OBO.

## Sprint 0

- [x] `manifest.xml`-Skelett mit TODO-Markern für Add-in-ID, Hosts, IconUrls
- [x] `package.json`-Skelett
- [ ] Yo-Office-Scaffold (oder Vite-Setup) durchziehen (TODO Sprint 1)

## Sprint 1 (MVP1)

- ItemRead Side Panel: Kontext (Kunde/Kontakt), Quellen, AI-Vorschlag.
- ItemSend/Compose: Antwortentwurf einfügen, **niemals automatisch senden**.
- Telemetrie an Backend-Endpunkt `/telemetry` (Audit-Events `ai.suggestion.*`).

## Build / Test (Sprint 1)

```powershell
pnpm install
pnpm dev          # Sideload gegen Outlook Web/Desktop
pnpm build
pnpm test
pnpm lint
```
