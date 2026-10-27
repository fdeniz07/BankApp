# 22 — Karar mektubu e-posta (lab)

[← Önceki](21-ci-github-actions.tr.md) · [İçindekiler](00-index.tr.md) · [Sonraki →](25-decision-letter-sms.tr.md)

İlgili: [25 SMS lab](25-decision-letter-sms.tr.md)

## Bu nedir?

Personel kredi başvurusunu **onayladığında veya reddettiğinde** müşteriye giden “karar mektubu” bildirimi.  
BankApp lab’inde gerçek SMTP yok: mesaj **RabbitMQ** kuyruğuna yazılır, consumer **Seq/Serilog**’a loglar (e-posta doğrulama ile aynı eğitim modeli).

## Bu projede ne işe yarar?

- UI’daki onay/red **notes** şablonları müşteriye iletilir (body).
- Outbox → `domain-events` akışına bağlıdır; SignalR toast’tan sonra tetiklenir.
- Kuyruk adı: `decision-letter`.

## Ne zaman eklendi?

Roadmap “İleride” maddesi — e-posta lab dilimi; SMS ayrı bölümde ([25](25-decision-letter-sms.tr.md)).

## Temel kavramlar

| Kavram | Anlamı |
|--------|--------|
| **Decision letter** | Onay/red sonrası müşteriye metin |
| **Publisher** | Mesajı kuyruğa basan servis |
| **Consumer** | Kuyruğu dinleyip “gönderim” simüle eden hosted service |
| **Lab delivery** | Log = e-posta yerine geçer (eğitim) |

## Komutlar

```powershell
cd infra
docker compose up -d rabbitmq webapi
# RabbitMQ UI → Queues → decision-letter
# Seq → "Decision letter consumed"
```

Broker kapalıysa publisher uyarı loglayıp body’yi yine yazar (kayıt/onay akışı kırılmaz).

## Sınıflar, metotlar, paketler

| İsim | Rol |
|------|-----|
| `DecisionLetterRequested` | Mesaj kaydı (ToEmail, Subject, Body, Decision, …) |
| `IDecisionLetterPublisher` | Soyutlama |
| `RabbitMqDecisionLetterPublisher` | Kuyruğa publish / disabled fallback log |
| `DecisionLetterConsumerHostedService` | Consume + Seq log |
| `DomainEventsConsumerHostedService.TryPublishDecisionLetterAsync` | Approve/Reject sonrası yayın |
| `RabbitMqOptions.DecisionLetterQueue` | Varsayılan `decision-letter` |

Paket: `RabbitMQ.Client` (mevcut Notification host).

## Repoda nerede?

```
src/Application/Services/Messaging/DecisionLetterRequested.cs
src/Application/Services/Messaging/IDecisionLetterPublisher.cs
src/WebApi/Modules/Notification/Messaging/RabbitMqDecisionLetterPublisher.cs
src/WebApi/Modules/Notification/Messaging/DecisionLetterConsumerHostedService.cs
src/WebApi/Modules/Notification/Messaging/DomainEventsConsumerHostedService.cs
src/WebApi/Modules/Notification/NotificationHostModule.cs
tests/Application.UnitTests/Services/Messaging/DecisionLetterRequestedTests.cs
```

## Mini akış

Staff onaylar → Outbox → `domain-events` → Saga + SignalR → müşteri e-postası bulunur → `DecisionLetterRequested` → `decision-letter` kuyruğu → consumer Seq’e `To` / `Subject` / `Body` yazar.
