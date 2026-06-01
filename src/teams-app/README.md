# Teams App

> Microsoft Teams App: Bot, Message Extension, Tab. Liefert Kontext und Antwortvorschläge im Teams-Kanal/-Chat – **kein Auto-Post**.

**Plandokumente:**
- [docs/plan/05-teams-app.md](../../docs/plan/05-teams-app.md) – Bot, ME, Tab, RSC
- [docs/plan/11-graph-feasibility.md §3](../../docs/plan/11-graph-feasibility.md) – RSC vs. Pay-per-use
- [docs/plan/12-security-compliance.md §3.1](../../docs/plan/12-security-compliance.md) – Resource-Specific Consent

**Owner:** `@TODO-org/communicationhub-frontend`

**Stack:** Teams Toolkit, TypeScript/React (Tab), Bot Framework v4 (Bot/ME). Auth via Teams SSO → Backend OBO.

## Struktur

```
manifest/
  manifest.json    Teams App Manifest (RSC-Permissions)
bot/               Bot + Message Extension (Bot Framework v4)
tab/               React Tab
extension/         Message Extension Spezifika (falls separat)
```

## Sprint 0

- [x] `manifest/manifest.json`-Skelett mit TODOs für IDs und RSC-Permissions
- [x] `package.json`-Skelett

## MVP2

- [ ] RSC-Manifest (`ChannelMessage.Read.Group`) für Pilot-Teams.
- [ ] Bot mit Adaptive Cards (Server-seitige Re-Authorization vor jeder Card-Ausgabe, [12-security §1.2](../../docs/plan/12-security-compliance.md)).
- [x] Backend-API für Teams Message Analysis angelegt (`POST /v1/teams/message/analyze`) als MVP2-Einstieg.
- [x] Backend-API für Teams Message Extension Preview angelegt (`POST /v1/teams/message/preview-interaction`) inkl. vorgeschlagenem `InteractionSaveRequest`.
- [ ] ME: Thread analysieren, Antwortentwurf einfügen, in BC-Timeline ablegen.
- [ ] Tab: Kunden-/Projekt-Arbeitsbereich (Timeline, Themenansicht, Dokumente).
