# 13 — RabbitMQ

[← Önceki](12-i18n-ux.tr.md) · [İçindekiler](00-index.tr.md) · [Sonraki →](14-outbox-events.tr.md)

## Bu nedir?

**RabbitMQ**: Mesaj kuyruğu. Üretici mesajı kuyruğa bırakır; tüketici (consumer) zamanı gelince işler.  
HTTP isteğini yavaş işlerden (e-posta simülasyonu, event yayını) ayırır.

## Bu projede ne işe yarar?

1. **E-posta doğrulama** kuyruğu (`email-confirmation`) — kayıt sonrası JWT hemen verilmez.  
2. **Domain events** kuyruğu (`domain-events`) — Outbox dispatcher yayınlar; Notification consumer okur → Saga + SignalR.

## Ne zaman eklendi?

E-posta kuyruğu: **Faz 5**. Domain-events: **Faz 6** (Outbox ile).

## Temel kavramlar

| Kavram | Anlamı |
|--------|--------|
| **Producer** | Mesajı yayınlayan |
| **Consumer** | Mesajı okuyan hosted service |
| **Queue** | Bekleme hattı |
| **Ack / Nack** | Mesajı başarı/başarısız işaretleme |
| **At-least-once** | En az bir kez teslim — consumer idempotent olmalı |

## Komutlar

```powershell
cd infra
docker compose up -d rabbitmq
# Management UI genelde http://localhost:15672 (compose’a bak)
```

UI’da Queues sekmesinde `email-confirmation`, `domain-events` görünür.

## Sınıflar, metotlar, paketler

| İsim | Rol |
|------|-----|
| `RabbitMQ.Client` | AMQP istemci |
| Email publisher / consumer (Identity/Notification) | Doğrulama maili (lab’de log) |
| `OutboxDispatcherHostedService` | Outbox → `domain-events` |
| `DomainEventsConsumerHostedService` | Kuyruk → saga + SignalR |

## Repoda nerede?

```
src/WebApi/Modules/Notification/Messaging/
infra/docker-compose.yml
docs/assets/screenshots/en/Queues on RabbitMQ.png
```

## Mini akış

Kayıt → token üret → mesaj kuyruğa → consumer log/Seq → kullanıcı e-posta linki ile confirm → sonra login.
