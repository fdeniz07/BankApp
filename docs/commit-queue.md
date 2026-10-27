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
| 2026-10-23 | 7 | Saga / Process Manager (domain → persist → manager → consumer → tests → docs) |
| 2026-10-23 | +5 | SignalR hub → consumer → CORS → React client → docs |
| 2026-10-24 | 5 | Modular Monolith (Application → WebApi host → tests → docs → queue) |
| 2026-10-24 | +2 | Tech learning guides (TR/EN/DE + README/architecture links → queue) |
| 2026-10-25 | 11 | OTel stack → toast/FX → income/DecisionBy → header name → docs |
| 2026-10-25 | +1 | Commit queue drain log |
| 2026-10-25 | +5 | Toast severity + payment plan PDF/print/export + READMEs |
| 2026-10-25 | +1 | Fix commit-queue encoding |
| 2026-10-25 | +1 | Decision letter templates + README Features/URLs |
| 2026-10-26 | 5 | Drain: PDF locale → toast currency → duplicate toasts → Scalar → screenshot gallery |
| 2026-10-26 | +2 | Learning library (`docs/learning/`) + track Cursor rules in git |
| 2026-10-26 | +N | CI fixes (NU1605, approve handler tests, monthlyIncome, RiskLevel syntax) |
| 2026-10-27 | 4 | Decision letter email lab (contract → consumer → test → docs) |
| 2026-10-27 | +1 | Commit queue drain log |
| 2026-10-27 | +3 | Rate-limit policies + Vault JWT overlay + docs |
| 2026-10-27 | +2 | Redis Insight Compose + lab credentials / learning docs |
| 2026-10-27 | +2 | Lab tools chapter (24) + annotated Compose recovery docs |
