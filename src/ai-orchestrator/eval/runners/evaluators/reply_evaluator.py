"""
C3 – Reply Suggestion Evaluator  (Sprint 0 skeleton, G2)

Evaluates capability C3 (ReplyWithCitations) on groundedness,
citation coverage and safety flags against goldlabel data.

Goldlabel record schema (JSONL):
{
  "id": "reply-001",
  "thread_snippet": "...",
  "context_sources": ["src://search/...", ...],
  "labels": {
    "reply_short": "...",
    "citations": ["src://..."],
    "contains_commitments": false,
    "tone": "formal"
  }
}

See: docs/plan/08-ai-orchestration.md §4 (Capability C3)
"""

from __future__ import annotations

import json
import logging
from typing import Any

from openai import AzureOpenAI
from azure.identity import DefaultAzureCredential, get_bearer_token_provider

log = logging.getLogger(__name__)


class ReplyEvaluator:
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
        """Run C3 reply evaluation and compute groundedness + citation metrics."""
        groundedness_scores: list[float] = []
        citation_coverage_scores: list[float] = []
        safety_false_positives = 0
        total = 0
        errors: list[dict[str, Any]] = []

        for rec in records:
            total += 1
            label = rec.get("labels", {})
            try:
                prediction = self._suggest_reply(rec)
            except Exception as exc:
                log.warning("Reply generation failed for %s: %s", rec.get("id"), exc)
                errors.append({"id": rec.get("id"), "error": str(exc)})
                continue

            # Groundedness: judge whether reply is supported by context_sources
            g_score = self._score_groundedness(
                reply=prediction.get("short_reply", ""),
                sources=rec.get("context_sources", []),
            )
            groundedness_scores.append(g_score)

            # Citation coverage: fraction of ground-truth citations present in prediction
            gt_citations: set[str] = set(label.get("citations", []))
            pred_citations: set[str] = set(prediction.get("citations", []))
            if gt_citations:
                coverage = len(gt_citations & pred_citations) / len(gt_citations)
            else:
                coverage = 1.0
            citation_coverage_scores.append(coverage)

            # Safety: track cases where model adds commitments/pricing when it shouldn't
            if prediction.get("contains_commitments") and not label.get("contains_commitments"):
                safety_false_positives += 1

        n = len(groundedness_scores)
        return {
            "metrics": {
                "groundedness": sum(groundedness_scores) / n if n else 0.0,
                "citation_coverage": sum(citation_coverage_scores) / n if n else 0.0,
                "safety_fp_rate": safety_false_positives / total if total else 0.0,
            },
            "counts": {"total": total, "evaluated": n, "errors": len(errors)},
            "errors": errors,
        }

    def _suggest_reply(self, record: dict[str, Any]) -> dict[str, Any]:
        """Call AOAI to generate a reply suggestion and parse the response."""
        import pathlib
        prompt_file = (
            pathlib.Path(__file__).parents[4] / "prompts" / "system" / "reply.md"
        )
        system_prompt = (
            prompt_file.read_text(encoding="utf-8")
            if prompt_file.exists()
            else "You are a professional reply drafting assistant."
        )
        user_msg = (
            f"Thread:\n{record.get('thread_snippet', '')[:1000]}\n\n"
            f"Available sources: {json.dumps(record.get('context_sources', []))}\n\n"
            "Draft a concise, formal reply with citations. "
            'Respond with JSON: {"short_reply": str, "citations": [str], "contains_commitments": bool}'
        )
        response = self._client.chat.completions.create(
            model=self._deployment,
            messages=[
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": user_msg},
            ],
            response_format={"type": "json_object"},
            temperature=0.2,
            max_tokens=512,
        )
        return json.loads(response.choices[0].message.content or "{}")

    def _score_groundedness(self, reply: str, sources: list[str]) -> float:
        """
        LLM-as-judge groundedness scoring (0.0 – 1.0).
        TODO MVP1 Sprint 1: replace with azure-ai-evaluation GroundednessEvaluator.
        """
        if not reply or not sources:
            return 0.0
        judge_prompt = (
            "You are a strict evaluator.\n"
            f"Sources: {json.dumps(sources)}\n"
            f"Reply: {reply}\n\n"
            "Rate how well the reply is supported by the sources (0.0 = not supported, 1.0 = fully supported). "
            'Respond with JSON: {"score": float, "reason": str}'
        )
        response = self._client.chat.completions.create(
            model=self._deployment,
            messages=[{"role": "user", "content": judge_prompt}],
            response_format={"type": "json_object"},
            temperature=0,
            max_tokens=128,
        )
        try:
            result = json.loads(response.choices[0].message.content or "{}")
            return float(result.get("score", 0.0))
        except (json.JSONDecodeError, ValueError):
            return 0.0
