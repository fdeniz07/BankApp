# BankApp Client

Vite + React + TypeScript frontend with TR / EN / DE i18n.

## Prerequisites

- Node.js 20+
- Backend API running (Docker WebApi on `http://localhost:8080` or Visual Studio)

## Setup

```bash
cd client
copy .env.example .env
npm install
npm run dev
```

Open http://localhost:3000

## Seed logins

- Staff: `staff@bank.local` / `Staff123!`
- Customer: `customer@bank.local` / `Customer123!`

## Docker (with full stack)

From repository root:

```bash
docker compose -f infra/docker-compose.yml --env-file infra/.env up -d --build
```

UI: http://localhost:3000  
API URL is baked at image build via `VITE_API_BASE_URL` (default `http://localhost:8080`).

## Features (MVP)

- Login + JWT session
- Language switcher (TR / EN / DE)
- Credit applications list + detail
- Staff approve / reject
- Credit types catalog (staff)
