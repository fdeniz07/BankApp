# 16 — SignalR

[← Previous](15-saga.md) · [Contents](00-index.md) · [Next →](17-modular-monolith.md)

## What is this?

**SignalR**: Real-time server → browser messages (WebSocket preferred).  
Toast / list refresh without reloading the page.

```mermaid
flowchart LR
  OB[Outbox consumer] --> HUB[/hubs/credit-applications]
  HUB --> ST[Staff groups]
  HUB --> CU[Customer groups]
  ST --> UI[Toast / list refresh]
```

## Role in BankApp

Hub: `/hubs/credit-applications`.  
When the outbox consumer handles an event, it pushes `CreditApplicationChanged` to staff and relevant customer groups (amount + **currencyCode**).

## When added?

**Phase 6** (implemented before Modular).

## Core concepts

| Concept | Meaning |
|--------|--------|
| **Hub** | Client–server contract endpoint |
| **Group** | Broadcast set by user/role |
| **access_token** | JWT on query/header when WebSocket connects |
| **Reconnect** | Retry after disconnect |

## Commands

```powershell
# Client package
cd client
npm install @microsoft/signalr
```

Browser DevTools → Network → WS to see hub traffic.

## Classes, methods, packages

| Name | Role |
|------|-----|
| Credit applications hub (WebApi) | Connection / groups |
| `ICreditApplicationRealtimeNotifier` | Publish abstraction |
| `CreditApplicationChangedMessage` | Payload (Amount, CurrencyCode) |
| `useCreditApplicationRealtime` | React hook + toast |

Packages: ASP.NET Core SignalR (server), `@microsoft/signalr` (client).

## Where in the repo?

```
src/WebApi/Modules/Notification/Realtime/
client/src/realtime/
```

## Mini flow

Staff approves → Outbox → Consumer → notify hub groups → customer browser shows success toast + `formatMoney(currencyCode)`.
