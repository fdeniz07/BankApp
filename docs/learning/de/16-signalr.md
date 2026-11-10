# 16 — SignalR

[← Zurück](15-saga.md) · [Inhalt](00-index.md) · [Weiter →](17-modular-monolith.md)

## Was ist das?

**SignalR**: **Sofortnachrichten** vom Server zum Browser (primär WebSocket).  
Toast / Listen-Update ohne Seitenreload.

```mermaid
flowchart LR
  OB[Outbox-Consumer] --> HUB[/hubs/credit-applications]
  HUB --> ST[Mitarbeiter-Gruppen]
  HUB --> CU[Kunden-Gruppen]
  ST --> UI[Toast / Listen-Refresh]
```

## Rolle in BankApp

Hub: `/hubs/credit-applications`.  
Outbox-Consumer verarbeitet Events und sendet `CreditApplicationChanged` an Staff- und Kundengruppen (amount + **currencyCode**).

## Wann hinzugefügt?

**Faz 6** (vor dem Modular-Monolith-Umbau).

## Grundbegriffe

| Begriff | Bedeutung |
|--------|--------|
| **Hub** | Vertragspunkt Client–Server |
| **Group** | Zielgruppe nach User/Rolle |
| **access_token** | JWT per Query/Header beim WebSocket |
| **Reconnect** | Wiederaufbau nach Verbindungsabbruch |

## Befehle

```powershell
cd client
npm install @microsoft/signalr
```

Browser DevTools → Network → WS zeigt Hub-Traffic.

## Klassen, Methoden, Pakete

| Name | Rolle |
|------|-----|
| Credit applications hub (WebApi) | Verbindung / Gruppen |
| `ICreditApplicationRealtimeNotifier` | Publish-Abstraktion |
| `CreditApplicationChangedMessage` | Payload (Amount, CurrencyCode) |
| `useCreditApplicationRealtime` | React-Hook + Toast |

Pakete: ASP.NET Core SignalR (Server), `@microsoft/signalr` (Client).

## Wo im Repo?

```
src/WebApi/Modules/Notification/Realtime/
client/src/realtime/
```

## Mini-Ablauf

Staff genehmigt → Outbox → Consumer → Hub-Gruppen benachrichtigen → Success-Toast beim Kunden + `formatMoney(currencyCode)`.
