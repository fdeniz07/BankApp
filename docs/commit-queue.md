# Commit queue

English, conventional-style commits (`feat:` / `fix:` / `docs:` / `test:` / `ci:` / `style:` / `refactor:` / `perf:` / `build:`  / `chore:`  / `revert:`), **max 20 commits per calendar day** (local timezone).

## Rules

1. Split each new feature into small, reviewable commits (one concern each).
2. Count today’s commits: `git log --since=midnight --oneline`.
3. Remaining capacity = `20 - todayCount`.
4. Commit up to remaining capacity now; append the rest under **Pending** below.
5. On a later day, drain **Pending** first (oldest first), then new work.
6. Do not force-push; do not amend unless the user asks.

## Pending

_(empty)_

## Drained

| Date | Count | Notes |
|------|------:|-------|
| 2026-10-21 | — | Queue created; outbox feature commits applied same day (under daily cap). |
