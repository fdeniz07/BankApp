# BankApp Client

Vite + React + TypeScript frontend with **TR / EN / DE** i18n.

## Prerequisites

- Node.js 20+
- Backend API running (Docker WebApi on `http://localhost:8080` or local `dotnet run`)

## Setup

```bash
cd client
copy .env.example .env
npm install
npm run dev
```

Open http://localhost:3000

## Demo logins

| Role | Email | Password |
|------|-------|----------|
| Staff | `staff.tr@` / `staff.de@` / `staff.en@bank.local` | `Demo123!` |
| Manager | `manager.tr@` / `manager.de@` / `manager.en@bank.local` | `Demo123!` |
| Customer | `customer.tr@` / `customer.de@` / `customer.en@bank.local` | `Demo123!` |
| SuperAdmin | `superadmin@bank.local` | `Demo123!` |

Locale variants: `staff.{tr\|en\|de}@bank.local`, `customer.{tr\|en\|de}@bank.local`.

## Docker (full stack)

From repository root:

```bash
docker compose -f infra/docker-compose.yml --env-file infra/.env up -d --build
```

- UI: http://localhost:3000  
- API base URL is baked at image build via `VITE_API_BASE_URL` (default `http://localhost:8080`)

## Features

- Login / individual register, JWT session
- Applications list & detail (approve / reject / cancel by role)
- Customers, credit types (manager create / activate), staff admin
- Profile + avatar crop, stats strip, FX ticker, accessibility toolbar
- Language switcher TR / EN / DE

See root [README.md](../README.md) for architecture and API overview.
