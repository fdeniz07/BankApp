# BankApp learning library

Beginner-friendly stack guide for technologies **actually used in this repo**, in **build order** (`01`–`34`).

Choose a language:

| Language | Index |
|----------|--------|
| **Türkçe** (source of truth) | [tr/00-index.md](tr/00-index.md) |
| **English** (full mirror `01`–`34`) | [en/00-index.md](en/00-index.md) |
| **Deutsch** (vollständiger Spiegel `01`–`34`) | [de/00-index.md](de/00-index.md) |

Related:

- Overview “why we chose it”: [tech-learning.tr.md](../tech-learning.tr.md) · [EN](../tech-learning.en.md) · [DE](../tech-learning.de.md)
- Changelog: [tr/99-changelog-learning.md](tr/99-changelog-learning.md) · [EN](en/99-changelog-learning.md) · [DE](de/99-changelog-learning.md)
- UI screenshot galleries: [Customer](../screenshots/customer.en.md) · [Staff](../screenshots/bank-employee.en.md) · [Manager](../screenshots/bank-manager.en.md) · [SuperAdmin](../screenshots/super-admin.en.md)

Layout:

```
docs/learning/
  README.md          # this hub
  tr/                # Turkish chapters (canonical)
  en/                # English mirrors (01–34 + index + changelog)
  de/                # German mirrors (01–34 + index + changelog)
```

## Keeping docs in sync with the repo (TR + EN + DE)

After **each approved** product or infra slice (not typo-only edits):

1. Update the matching chapter(s) under `tr/` — beginner sections, commands, pitfalls, repo paths.
2. Mirror the same content in `en/` and `de/` when those chapters exist.
3. Append one line to `tr/99-changelog-learning.md` (+ EN/DE changelogs).
4. If the slice is a product milestone, also add a bullet to `docs/history.{tr,en,de}.md`.
5. Point ops detail at `infra/README.md`; teach the *why/how* here, not duplicate entire compose files.

Typical chapter map: Compose/ops → [09](tr/09-docker-compose.md) + [24](tr/24-lab-tools.md); Vault → [19](tr/19-vault-lab.md); Redis → [08](tr/08-redis.md); ELK → [31](tr/31-elk-elasticsearch.md).
