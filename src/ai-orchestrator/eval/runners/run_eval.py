#!/usr/bin/env python3
"""
Communication Copilot – Evaluation Runner Skeleton
Sprint 0, WS-G Task G2

Runs offline evaluations against the goldlabel corpus (datasets/) and scores
AI capabilities C1 (classification), C2 (extraction), C3 (reply suggestion),
C4 (single-message summary) against ground-truth labels.

Usage:
    python run_eval.py --capability c1 --dataset datasets/goldlabel-c1.jsonl
    python run_eval.py --all --dataset-dir datasets/ --output-dir results/
    python run_eval.py --capability c1 --dataset datasets/goldlabel-c1.jsonl --adversarial

See: docs/plan/08-ai-orchestration.md, docs/plan/16-testing-acceptance.md §6
"""

from __future__ import annotations

import argparse
import json
import logging
import os
import sys
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

from evaluators.classification_evaluator import ClassificationEvaluator
from evaluators.reply_evaluator import ReplyEvaluator

# ---------------------------------------------------------------------------
# Logging
# ---------------------------------------------------------------------------
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s – %(message)s",
)
log = logging.getLogger("eval_runner")

# ---------------------------------------------------------------------------
# Constants
# ---------------------------------------------------------------------------
CAPABILITIES = ("c1", "c2", "c3", "c4")

EVALUATOR_MAP: dict[str, Any] = {
    "c1": ClassificationEvaluator,
    "c3": ReplyEvaluator,
    # TODO MVP1 Sprint 1: add C2 (ExtractionEvaluator) and C4 (SummaryEvaluator)
}

# Acceptance thresholds from docs/plan/16-testing-acceptance.md §6
PASS_THRESHOLDS: dict[str, float] = {
    "c1_accuracy": 0.90,
    "c3_groundedness": 0.85,
    "c3_citation_coverage": 0.80,
    "c2_field_f1": 0.85,
    "c4_rouge2": 0.30,
}


# ---------------------------------------------------------------------------
# Core runner
# ---------------------------------------------------------------------------

def load_dataset(path: Path) -> list[dict[str, Any]]:
    """Load a JSONL goldlabel dataset file."""
    if not path.exists():
        raise FileNotFoundError(f"Dataset not found: {path}")
    records = []
    with path.open(encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if line:
                records.append(json.loads(line))
    log.info("Loaded %d records from %s", len(records), path)
    return records


def run_capability(
    capability: str,
    dataset_path: Path,
    aoai_endpoint: str,
    aoai_key: str | None,
    model_deployment: str,
    adversarial: bool = False,
) -> dict[str, Any]:
    """Run evaluation for a single capability. Returns a result dict."""
    EvaluatorClass = EVALUATOR_MAP.get(capability)
    if EvaluatorClass is None:
        log.warning("No evaluator registered for capability %s – skipping.", capability)
        return {"capability": capability, "status": "skipped", "reason": "no_evaluator"}

    records = load_dataset(dataset_path)
    if adversarial:
        adv_path = dataset_path.parent.parent / "adversarial"
        adv_files = list(adv_path.glob(f"*{capability}*.jsonl"))
        for adv_file in adv_files:
            records += load_dataset(adv_file)
        log.info("Added adversarial examples – total %d records", len(records))

    evaluator = EvaluatorClass(
        aoai_endpoint=aoai_endpoint,
        aoai_key=aoai_key,
        model_deployment=model_deployment,
    )

    log.info("Running %s evaluator on %d records …", capability.upper(), len(records))
    results = evaluator.evaluate(records)

    # Check against acceptance thresholds
    passed = True
    for metric, threshold in PASS_THRESHOLDS.items():
        if not metric.startswith(capability):
            continue
        metric_key = metric.split("_", 1)[1]
        score = results.get("metrics", {}).get(metric_key)
        if score is None:
            continue
        ok = score >= threshold
        results.setdefault("gate_checks", {})[metric_key] = {
            "score": score,
            "threshold": threshold,
            "passed": ok,
        }
        if not ok:
            passed = False
            log.warning(
                "GATE FAIL – %s.%s: %.3f < %.3f", capability, metric_key, score, threshold
            )

    results["capability"] = capability
    results["dataset"] = str(dataset_path)
    results["adversarial"] = adversarial
    results["gate_passed"] = passed
    results["run_timestamp"] = datetime.now(timezone.utc).isoformat()
    return results


def write_results(results: list[dict[str, Any]], output_dir: Path) -> Path:
    """Write evaluation results as JSON and a human-readable summary."""
    output_dir.mkdir(parents=True, exist_ok=True)
    ts = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")
    out_file = output_dir / f"eval-results-{ts}.json"
    with out_file.open("w", encoding="utf-8") as f:
        json.dump(results, f, indent=2, ensure_ascii=False)
    log.info("Results written to %s", out_file)

    # Summary to stdout
    print("\n" + "=" * 60)
    print("EVALUATION SUMMARY")
    print("=" * 60)
    for r in results:
        cap = r.get("capability", "?")
        status = r.get("status", "completed")
        gate = r.get("gate_passed")
        gate_str = "✓ PASS" if gate else ("✗ FAIL" if gate is not None else "–")
        print(f"  {cap.upper():<6} {status:<12} {gate_str}")
        for name, check in r.get("gate_checks", {}).items():
            mark = "✓" if check["passed"] else "✗"
            print(f"         {mark} {name}: {check['score']:.3f} (threshold {check['threshold']})")
    print("=" * 60 + "\n")

    return out_file


# ---------------------------------------------------------------------------
# CLI
# ---------------------------------------------------------------------------

def build_parser() -> argparse.ArgumentParser:
    p = argparse.ArgumentParser(
        description="Communication Copilot offline evaluation runner"
    )
    target = p.add_mutually_exclusive_group(required=True)
    target.add_argument("--capability", choices=CAPABILITIES, help="Single capability to evaluate.")
    target.add_argument("--all", action="store_true", help="Run all capabilities.")

    p.add_argument(
        "--dataset",
        type=Path,
        help="Path to a JSONL goldlabel dataset (required for --capability).",
    )
    p.add_argument(
        "--dataset-dir",
        type=Path,
        default=Path(__file__).parent.parent / "datasets",
        help="Directory containing goldlabel JSONL files (used with --all).",
    )
    p.add_argument(
        "--output-dir",
        type=Path,
        default=Path(__file__).parent.parent / "results",
        help="Directory to write result JSON files.",
    )
    p.add_argument("--adversarial", action="store_true", help="Include adversarial examples.")
    p.add_argument(
        "--model-deployment",
        default=os.environ.get("EVAL_MODEL_DEPLOYMENT", "gpt-4.1-eu"),
    )
    return p


def main() -> int:
    parser = build_parser()
    args = parser.parse_args()

    aoai_endpoint = os.environ.get("AZURE_OPENAI_ENDPOINT", "")
    aoai_key = os.environ.get("AZURE_OPENAI_API_KEY")  # None → uses Managed Identity

    if not aoai_endpoint:
        log.error("AZURE_OPENAI_ENDPOINT environment variable not set.")
        return 1

    capabilities_to_run: list[str]
    dataset_paths: dict[str, Path]

    if args.all:
        capabilities_to_run = list(CAPABILITIES)
        dataset_dir: Path = args.dataset_dir
        dataset_paths = {
            cap: dataset_dir / f"goldlabel-{cap}.jsonl"
            for cap in capabilities_to_run
        }
    else:
        if args.dataset is None:
            parser.error("--dataset is required when using --capability")
        capabilities_to_run = [args.capability]
        dataset_paths = {args.capability: args.dataset}

    all_results = []
    exit_code = 0
    for cap in capabilities_to_run:
        ds_path = dataset_paths[cap]
        try:
            result = run_capability(
                capability=cap,
                dataset_path=ds_path,
                aoai_endpoint=aoai_endpoint,
                aoai_key=aoai_key,
                model_deployment=args.model_deployment,
                adversarial=args.adversarial,
            )
        except FileNotFoundError as exc:
            log.warning("Skipping %s – %s", cap, exc)
            result = {"capability": cap, "status": "skipped", "reason": str(exc)}
        except Exception:
            log.exception("Evaluator for %s raised an unexpected error.", cap)
            result = {"capability": cap, "status": "error"}
            exit_code = 1

        all_results.append(result)
        if result.get("gate_passed") is False:
            exit_code = 1

    write_results(all_results, args.output_dir)
    return exit_code


if __name__ == "__main__":
    sys.exit(main())
