"""
C1 – Classification Evaluator  (Sprint 0 skeleton, G2)

Evaluates capability C1 (IsExternalCommunication + ChannelType + SensitivityLevel)
against goldlabel data.

Goldlabel record schema (JSONL):
{
  "id": "msg-001",
  "subject": "RE: Angebot ...",
  "snippet": "...",
  "participants": [{"email": "...", "role": "From"}, ...],
  "labels": {
    "is_external": true,
    "channel": "Email",
    "sensitivity": "Internal"
  }
}

See: docs/plan/08-ai-orchestration.md §2 (Capability C1)
"""

from __future__ import annotations

import logging
from typing import Any

from openai import AzureOpenAI
from azure.identity import DefaultAzureCredential, get_bearer_token_provider

log = logging.getLogger(__name__)

SYSTEM_PROMPT_PATH = (
    __file__
    + "/../../../../prompts/system/classification.md"
)


class ClassificationEvaluator:
    def __init__(
        self,
        aoai_endpoint: str,
        aoai_key: str | None,
        model_deployment: str,
    ) -> None:
        if aoai_key:
            self._client = AzureOpenAI(
                azure_endpoint=aoai_endpoint,
                api_key=aoai_key,
                api_version="2024-12-01-preview",
            )
        else:
            token_provider = get_bearer_token_provider(
                DefaultAzureCredential(),
                "https://cognitiveservices.azure.com/.default",
            )
            self._client = AzureOpenAI(
                azure_endpoint=aoai_endpoint,
                azure_ad_token_provider=token_provider,
                api_version="2024-12-01-preview",
            )
        self._deployment = model_deployment

    def evaluate(self, records: list[dict[str, Any]]) -> dict[str, Any]:
        """Run C1 classification on all records and compute accuracy metrics."""
        tp = fp = tn = fn = 0
        channel_correct = sensitivity_correct = total = 0
        errors: list[dict[str, Any]] = []

        for rec in records:
            total += 1
            label = rec.get("labels", {})
            try:
                prediction = self._classify(rec)
            except Exception as exc:
                log.warning("Classification failed for %s: %s", rec.get("id"), exc)
                errors.append({"id": rec.get("id"), "error": str(exc)})
                continue

            # is_external binary metric
            actual_ext = label.get("is_external", False)
            pred_ext = prediction.get("is_external", False)
            if actual_ext and pred_ext:
                tp += 1
            elif actual_ext and not pred_ext:
                fn += 1
            elif not actual_ext and pred_ext:
                fp += 1
            else:
                tn += 1

            # channel and sensitivity
            if prediction.get("channel") == label.get("channel"):
                channel_correct += 1
            if prediction.get("sensitivity") == label.get("sensitivity"):
                sensitivity_correct += 1

        precision = tp / (tp + fp) if (tp + fp) > 0 else 0.0
        recall = tp / (tp + fn) if (tp + fn) > 0 else 0.0
        f1 = (2 * precision * recall / (precision + recall)) if (precision + recall) > 0 else 0.0
        accuracy = (tp + tn) / total if total > 0 else 0.0

        return {
            "metrics": {
                "accuracy": accuracy,
                "precision": precision,
                "recall": recall,
                "f1": f1,
                "channel_accuracy": channel_correct / total if total > 0 else 0.0,
                "sensitivity_accuracy": sensitivity_correct / total if total > 0 else 0.0,
            },
            "counts": {"total": total, "tp": tp, "fp": fp, "tn": tn, "fn": fn},
            "errors": errors,
        }

    def _classify(self, record: dict[str, Any]) -> dict[str, Any]:
        """Call AOAI and parse the structured classification response."""
        # TODO MVP1 Sprint 1: replace with structured output / function calling
        # to get a typed ClassificationResult object.
        prompt = _build_classification_prompt(record)
        response = self._client.chat.completions.create(
            model=self._deployment,
            messages=[
                {"role": "system", "content": _load_system_prompt()},
                {"role": "user", "content": prompt},
            ],
            response_format={"type": "json_object"},
            temperature=0,
            max_tokens=256,
        )
        import json
        return json.loads(response.choices[0].message.content or "{}")


def _build_classification_prompt(record: dict[str, Any]) -> str:
    parts = [f"Subject: {record.get('subject', '')}"]
    if record.get("snippet"):
        parts.append(f"Snippet: {record['snippet'][:400]}")
    participants = record.get("participants", [])
    if participants:
        parts.append("Participants: " + "; ".join(
            f"{p.get('email', '?')} ({p.get('role', '?')})" for p in participants[:10]
        ))
    parts.append(
        "\nClassify this communication. Respond with JSON: "
        '{"is_external": bool, "channel": str, "sensitivity": str}'
    )
    return "\n".join(parts)


def _load_system_prompt() -> str:
    import pathlib, os
    prompt_file = pathlib.Path(__file__).parents[4] / "prompts" / "system" / "classification.md"
    if prompt_file.exists():
        return prompt_file.read_text(encoding="utf-8")
    return "You are a communication classification assistant."
