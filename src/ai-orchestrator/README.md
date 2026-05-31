# AI Orchestrator

> Prompt-Repository und Eval-Runner für die Capabilities C1–C8 des Customer Communication Copilots.

**Plandokumente:**
- [docs/plan/08-ai-orchestration.md](../../docs/plan/08-ai-orchestration.md) – Capabilities, Modell-Routing, Eval-Framework
- [docs/plan/16-testing-acceptance.md §6](../../docs/plan/16-testing-acceptance.md) – Eval-Schwellen je MVP

**Owner:** `@TODO-org/communicationhub-ai`

## Struktur

```
prompts/
  system/          Gehärtete System-Prompts (Trust-Boundary "trusted system")
  capabilities/    C1 Klassifikation, C2 Extraktion, C3 Antwortvorschlag, …
eval/
  datasets/        Goldlabel-Korpora (300 Mails / 50 Threads für MVP1)
  runners/         Eval-Runner (Regression, Faithfulness, F1, Coverage)
  adversarial/     Prompt-Injection / Jailbreak / Tenant-Leakage-Tests
```

## Sprint 0

- [x] Ordnerstruktur
- [ ] Eval-Runner-Stack festlegen (Promptflow vs. eigene Pipeline) (TODO)

## Sprint 1 (MVP1 – C1–C4)

- System-Prompts mit Trust-Boundary (untrusted Mail-Body sauber gekapselt).
- Capability-Prompts C1 Klassifikation, C2 Extraktion, C3 Antwortvorschlag (mit Citations), C4 Mail-Summary.
- Goldlabel-Korpus: 300 Mails / 50 Threads.
- Adversarial-Set für Prompt-Injection (siehe [12-security §12](../../docs/plan/12-security-compliance.md)).
- CI-Trigger: `.github/workflows/ai-eval.yml` (manuell + bei Prompt-Änderungen).

## Eval-Schwellen MVP1

- **C1 Klassifikation:** Accuracy ≥ 0,80
- **C2 Extraktion:** F1 ≥ 0,80 (Kunde/Beleg)
- **C3 Antwortvorschlag:** Faithfulness ≥ 0,95
- **C4 Summary:** Coverage ≥ 0,90
