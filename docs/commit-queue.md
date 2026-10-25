# Commit queue

English, conventional-style commits (`feat:` / `fix:` / `doc:` / `test:` / `ci:`), **max 20 commits per calendar day** (local timezone).

## Rules

1. Split each new feature into small, reviewable commits (one concern each).
2. Count today's commits: `git log --since=midnight --oneline`.
3. Remaining capacity = `20 - todayCount`.
4. Commit up to remaining capacity now; append the rest under **Pending** below.
5. On a later day, drain **Pending** first (oldest first), then new work.
6. Do not force-push; do not amend unless the user asks.

## Pending

_(empty)_

## Drained

| Date | Count | Notes |
|------|------:|-------|
| 2026-10-21 | 13 | Outbox feature + commit-queue convention |
| 2026-10-22 | ? | Vault Compose lab + Idempotency-Key (after midnight) |
| 2026-10-23 | 7 | Saga / Process Manager (domain ? persist ? manager ? consumer ? tests ? docs) |
| 2026-10-23 | +5 | SignalR hub ? consumer ? CORS ? React client ? docs |
| 2026-10-24 | 5 | Modular Monolith (Application ? WebApi host ? tests ? docs ? queue) |
| 2026-10-24 | +2 | Tech learning guides (TR/EN/DE + README/architecture links ? queue) |
| 2026-10-25 | 11 | OTel stack ? toast/FX ? income/DecisionBy ? header name ? docs |
| 2026-10-25 | +1 | Commit queue drain log |
| 2026-10-25 | +5 | Toast severity + payment plan PDF/print/export + READMEs |
| 2026-10-25 | +1 | Fix commit-queue encoding |
