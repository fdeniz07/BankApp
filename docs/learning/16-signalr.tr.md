# 16 — SignalR

[← Önceki](15-saga.tr.md) · [İçindekiler](00-index.tr.md) · [Sonraki →](17-modular-monolith.tr.md)

## Bu nedir?

**SignalR**: Sunucudan tarayıcıya **anlık** mesaj (WebSocket öncelikli).  
Sayfa yenilemeden toast / liste yenileme.

## Bu projede ne işe yarar?

Hub: `/hubs/credit-applications`.  
Outbox consumer olay işleyince staff + ilgili müşteri gruplarına `CreditApplicationChanged` basar (amount + **currencyCode**).

## Ne zaman eklendi?

**Faz 6** (Modular’dan önce uygulanmıştı).

## Temel kavramlar

| Kavram | Anlamı |
|--------|--------|
| **Hub** | İstemci-sunucu sözleşme noktası |
| **Group** | Kullanıcı/rol bazlı yayın kümesi |
| **access_token** | WebSocket bağlanırken JWT query/header |
| **Reconnect** | Bağlantı kopunca yeniden deneme |

## Komutlar

```powershell
# Client paket
cd client
npm install @microsoft/signalr
```

Browser DevTools → Network → WS ile hub trafiğini görürsün.

## Sınıflar, metotlar, paketler

| İsim | Rol |
|------|-----|
| Credit applications hub (WebApi) | Bağlantı / grup |
| `ICreditApplicationRealtimeNotifier` | Publish soyutlaması |
| `CreditApplicationChangedMessage` | Payload (Amount, CurrencyCode) |
| `useCreditApplicationRealtime` | React hook + toast |

Paketler: ASP.NET Core SignalR (sunucu), `@microsoft/signalr` (istemci).

## Repoda nerede?

```
src/WebApi/Modules/Notification/Realtime/
client/src/realtime/
```

## Mini akış

Staff onaylar → Outbox → Consumer → Notify hub groups → müşteri tarayıcısında success toast + formatMoney(currencyCode).
