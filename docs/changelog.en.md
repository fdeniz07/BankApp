# Changelog

[Türkçe](changelog.tr.md) · [Deutsch](changelog.de.md)

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
