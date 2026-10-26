# 14 — Domain Events ve Transactional Outbox

[← Önceki](13-rabbitmq.tr.md) · [İçindekiler](00-index.tr.md) · [Sonraki →](15-saga.tr.md)

## Bu nedir?

**Domain Event**: Aggregate içinde önemli bir şey oldu (`CreditApplicationApproved`).  
**Transactional Outbox**: Event’i **aynı DB transaction** içinde `OutboxMessages` tablosuna yaz; sonra ayrı worker RabbitMQ’ya bas. Böylece “DB evet / kuyruk hayır” yarışı azalır.

## Bu projede ne işe yarar?

Kredi create / approve / reject / cancel sonrası güvenilir yayın → Saga güncelleme + SignalR toast.

## Ne zaman eklendi?

**Faz 6**.

## Temel kavramlar

| Kavram | Anlamı |
|--------|--------|
| **IDomainEvent** | Olay sözleşmesi |
| **Outbox row** | Henüz publish edilmemiş olay kaydı |
| **Dispatcher** | Periyodik okuyup kuyruğa basan hosted service |
| **Dual-write problem** | İki farklı depolama yeri tutarsızlığı |

## Komutlar

Özel CLI yok. Gözlem:

```powershell
# SQL’de outbox (lab connection ile)
# SELECT TOP 20 * FROM OutboxMessages ORDER BY OccurredOnUtc DESC

docker compose logs -f webapi
```

## Sınıflar, metotlar, paketler

| İsim | Rol |
|------|-----|
| `CreditApplicationApproved` vb. | Domain event tipleri |
| `OutboxMessage` entity | Tablo satırı |
| `OutboxDispatcherHostedService` | Publish |
| `DomainEventsConsumerHostedService` | Consume |
| Aggregate `Approve(...)` | Event üretir |

## Repoda nerede?

```
src/Domain/Events/
src/Persistence/…Outbox…
src/WebApi/…/Outbox…
src/WebApi/Modules/Notification/Messaging/DomainEventsConsumerHostedService.cs
```

## Mini akış

Handler SaveChanges (aggregate + outbox) → Dispatcher publish → RabbitMQ → Consumer → ProcessManager + SignalR `NotifyAsync`.
