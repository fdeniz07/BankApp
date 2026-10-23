# Changelog

[Türkçe](changelog.tr.md) · [Deutsch](changelog.de.md)

## 2026-10-22 — Saga / Process Manager (credit applications)

- `CreditApplicationSaga` + SmartEnum status/step; table `CreditApplicationSagas`.
- `CreditApplicationProcessManager`: Created → AwaitingDecision; Approved → Risk/Contract/Notify → Completed; Rejected/Cancelled → Terminated.
- Domain events consumer wired to the process manager; unit tests.

## 2026-10-22 — HashiCorp Vault lab (Docker Compose)

- Compose: `vault` (dev mode, :8200) + one-shot `vault-seed`.
- KV `secret/bankapp`: sample JWT / SQL / RabbitMQ secrets.
- WebApi does not read Vault yet; `infra/README` lab steps.

## 2026-10-21 — Idempotency-Key (credit write APIs)

- Required header `Idempotency-Key` on create / approve / reject / cancel.
- Redis response cache (24h); same key+body → replay; different body → 409.
- React client sends UUID header; integration tests + lab notes.

## 2026-10-21 — Domain Events + Transactional Outbox

- `AggregateRoot` + credit application events (Created / Approved / Rejected / Cancelled).
- `OutboxMessages` table; written in the same transaction as `BankingDbContext.SaveChanges`.
- `OutboxDispatcherHostedService` → RabbitMQ queue `domain-events`; consumer logs to Seq/console.
- Unit + integration tests (outbox rows).

## 2026-10-21 — Phase 6 architecture roadmap + discussion notes

- Roadmap: **Phase 6 — Architectural deepening** (Outbox, Idempotency, Saga, Modular Monolith, SignalR).
- Discussion archive: [architecture-notes.en.md](architecture-notes.en.md) (TR/DE counterparts).
- History and README doc links updated.

## 2026-10-19 — Docker Compose health checks

- Redis / Seq / WebApi (`/health`) / Client healthchecks; `depends_on: service_healthy` chain.
- WebApi image includes `curl`; liveness only (no DB/Redis probes).

## 2026-10-19 — Customer create/detail field parity + catalog suggestions

- Individual/corporate create forms aligned with detail fields (password only on create).
- Corporate type/sector/industry/sub-industry: datalist (pick or type new); `GET …/catalog`.

## 2026-10-19 — Auto customer number / read-only

- Staff create: system assigns number (`IND|CORP-{TR|EN|DE}-…`); no form field.
- Detail/update: number visible, not editable (API ignores changes).

## 2026-10-19 — Customer number locale + staff detail fields

- Registration customer no: `IND-{TR|EN|DE}-{yyyyMMdd}-{hex}` (UI language).
- Staff customer detail: individual DOB/parents; corporate company email/address/type/sector/industry/foundation.

## 2026-10-19 — Remember me + customer profile fields

- Login: “Remember me” → longer JWT (7 days) + `localStorage`; unchecked → short JWT + `sessionStorage`; remembered email.
- Customer profile: individual (DOB / parents + read-only number/national id), corporate (tax / registration fields).

## 2026-10-19 — Email confirmation (RabbitMQ)

- Individual register no longer returns a JWT; `EmailConfirmed` + token (24h); customer login requires confirmation.
- `POST /api/Auth/confirm-email`; client `/confirm-email?token=…`.
- RabbitMQ queue (Compose); if the broker is down the link is logged to Seq/console.
- Existing / seed users are confirmed; new registrations wait for confirmation.

## 2026-10-19 — Privacy / cookie consent

- First-load privacy/cookie banner (Accept / Decline); preference in `localStorage` (`bankapp-consent`).
- TR / EN / DE copy; layout clears the accessibility FAB.

## 2026-10-18 — CI + integration tests

- GitHub Actions CI: `dotnet build/test` (unit + integration) and client npm build.
- `WebApi.IntegrationTests`: InMemory DB + memory cache factory; login smoke.
- Flows: create→approve, create→reject, staff/customer cancel; customer cannot approve (401); unauthenticated create → 401.

## 2026-10-16 — Accessibility, FX ticker, Details button

- Bottom-left accessibility panel: light/dark theme, font size, high contrast (`localStorage`).
- Top scrolling FX band (USD/TRY, EUR/TRY, etc.; open.er-api with fallbacks).
- List **Details** button slate-blue (`btn info`); action spacing increased.

## 2026-10-16 — Profile crop, title/gender, fixed positions, header avatar

- Avatar: drag / zoom / center in frame, then upload.
- Academic title (`Dr`/`Prof`) separated from gender.
- Staff position from fixed SmartEnum list (localized labels).
- Circular avatar in top bar; name/image → `/profile`.

## 2026-10-16 — Manager credit type management

- Create / Update / Delete restricted to `BankManager`.
- `PUT /api/CreditTypes/{id}/active` for activate/deactivate.
- New applications blocked when parent credit type is inactive.
- UI: Manager create form + row activate/deactivate.

## 2026-10-16 — Profile, avatar, Seq audit

- `ApplicationUser` profile fields: FirstName, LastName, PersonTitle (SmartEnum), DateOfBirth, Position, ProfileImagePath.
- Staff can update own profile; managers edit all staff (email/password/status/role).
- Avatar upload: JPEG/PNG/WebP, max 2 MB, magic-byte validation; `wwwroot/uploads/profiles`.
- Structured Seq audit: Auth / Credit / Staff / Profile (`EventCategory`, `EventAction`).
- Email must not contain Turkish characters; JWT `nameidentifier` claim fix (SOAP URI).

## 2026-10-16 — Dashboard statistics (all roles)

- `GET /api/Dashboard/summary`: staff sees active/inactive customers + application breakdown; customers see their own application counts.
- Stats strip on every page (customer count for staff; applications/pending for everyone).
- Role-based dashboard panel on applications page; counts from API (not current page rows).

## 2026-10-16 — Customer management, BankManager, individual register

- `BankManager` claim + demo `manager@bank.local` / `Manager123!` (BankStaff + BankManager).
- Soft-delete only for BankManager; customer must be inactive and have no Pending/Approved applications.
- `Gender` allow-list (`Female` / `Male` / `PreferNotToSay`) on create / update / register.
- Public `POST /api/Auth/register-individual` + `/register` page.
- Staff `/customers` list (active / inactive / deleted, individual / corporate), create form, and detail (IsActive, delete).

## 2026-10-16 — UX bugfix (spacing, locale, category, toast)

- Stronger Name/Code column spacing on credit types.
- Login writes `displayName` / `customerCategory` via local override (fixes Turkish chars without relying on JWT decode alone).
- Seed monthly income scaled to requested amount.
- Customer create form lists only matching Individual/Corporate products; API `category` filter.
- Removed empty `CustomerId` create-validation block; `react-toastify` + localized error messages.

## 2026-10-16 — UX fixes, profile, JWT UTF-8

- Credit types list: column sorting (`SortBy` / `SortDesc`) and Name/Code spacing.
- Application detail shows approved term; payment plan states the effective amount/term (Pending preview / Approved values).
- Seed approved term never exceeds requested; often equals requested.
- JWT `displayName` UTF-8-safe decode; `customerCategory` claim; meaningful staff display names.
- `/profile` page for customer name/contact edits; header shows customer type.
- Roadmap Later: email verification (RabbitMQ/Kafka), Elasticsearch, Tailwind admin dashboard.

## 2026-10-16 — Locale catalog, package constraints, payment plan

- Added `CreditType.Locale`; reseeds TR→TRY, EN→USD, DE→EUR catalogs.
- Customer UI shows only products for the active UI locale; amount/term come from package options.
- Staff credit-types page supports TR/EN/DE filter.
- Server-side package validation (amount grid + allowed terms 3…60).
- Amortization payment-plan API + UI preview.
- Login `displayName` (full name / company); locale-specific demo accounts.
- SQL volume + Redis reset (clean seed).
- Docs: changelog, test notes, roadmap/history updates.
