# 25 — Decision letter SMS (lab)

[← Previous](24-lab-tools.md) · [Contents](00-index.md) · [Next →](26-credit-application-documents.md)

Related: [22 E-mail lab](22-decision-letter-email.md) · [13 RabbitMQ](13-rabbitmq.md) · [07 Seq](07-serilog-seq.md)

## What is this?

**SMS** notification to the customer after approve/reject.  
No real SMS gateway (Twilio, etc.): message goes to **RabbitMQ** → consumer **logs to Seq** (same teaching model as the e-mail lab).

**Analogy:** Same “write to the lab notebook” idea as e-mail — the courier is RabbitMQ; Seq is the notebook line for a short SMS text.

```mermaid
flowchart LR
  EV["Approve or reject event"] --> PUB["SMS Publisher"]
  PHN["Customer.Phone"] --> PUB
  PUB --> Q["decision-letter-sms"]
  Q --> CON["SMS Consumer"]
  CON --> SEQ["Seq log"]
```

```mermaid
sequenceDiagram
  participant Event as domain-events
  participant Pub as SMS publisher
  participant Q as decision-letter-sms
  participant Cons as SMS consumer
  participant Seq
  Event->>Pub: Approve / Reject
  Pub->>Q: DecisionSmsRequested
  Q->>Cons: Deliver
  Cons->>Seq: Log To / Body
```

## Role in BankApp

- Fires **together with** the e-mail decision letter (approve/reject domain event).
- Phone from `Customer.PhoneNumber`; if missing → skip + warning (flow continues).
- Queue: `decision-letter-sms`.
- Short body (lab max ~320 chars; from notes when present).

## When added?

Roadmap “Send decision letter via SMS” — easy slice after e-mail lab.

## Core concepts

| Concept | Meaning |
|--------|--------|
| **Decision SMS** | Short approve/reject text (phone) |
| **Lab delivery** | Seq log stands in for SMS |
| **Skip** | No publish when phone missing |

## Commands

```powershell
cd infra
docker compose --env-file .env up -d rabbitmq webapi
# Check: RabbitMQ UI → Queues → decision-letter-sms (Consumers ≥ 1)
```

```powershell
# After approve/reject
# Seq: "Decision SMS consumed"  (To= phone, Body=…)
docker compose --env-file .env logs --tail=50 webapi
```

If the broker is down, the publisher logs a warning but still writes the body (registration/approve does not break).

### What to verify

| Check | Good sign |
|---------|-------------|
| Queue `decision-letter-sms` | Consumers ≥ 1 |
| Approve/reject | Seq shows e-mail + SMS lines |
| Customer with empty phone | Warning skip; e-mail may still go |

## Classes, methods, packages

| Name | Role |
|------|-----|
| `DecisionSmsRequested` | ToPhone, Body, Decision, … |
| `IDecisionSmsPublisher` / `RabbitMqDecisionSmsPublisher` | Publish to queue |
| `DecisionSmsConsumerHostedService` | Consume → Seq |
| `TryPublishDecisionSmsAsync` | Inside domain events consumer |
| `RabbitMqOptions.DecisionSmsQueue` | Default `decision-letter-sms` |

## Where in the repo?

```
src/Application/Services/Messaging/DecisionSmsRequested.cs
src/Application/Services/Messaging/IDecisionSmsPublisher.cs
src/WebApi/Modules/Notification/Messaging/RabbitMqDecisionSmsPublisher.cs
src/WebApi/Modules/Notification/Messaging/DecisionSmsConsumerHostedService.cs
src/WebApi/Modules/Notification/Messaging/DomainEventsConsumerHostedService.cs
src/WebApi/Modules/Notification/NotificationHostModule.cs
tests/.../DecisionSmsRequestedTests.cs
```

## Mini flow

Staff approves → Outbox → `domain-events` → SignalR + decision-letter e-mail + **decision-letter-sms** → consumer writes `To` / `Body` to Seq.

## Beginner lab

1. RabbitMQ UI: queue `decision-letter-sms` listed.
2. Approve a Pending app for a customer with phone.
3. Seq: `Decision SMS` line with `To=` and `Body=`.
4. Also see e-mail queue log ([22](22-decision-letter-email.md)).
5. Reject for customer without phone → warning skip.

## Common mistakes

- Checking only **e-mail** queue — SMS uses `decision-letter-sms`.
- Searching Seq for **e-mail subject** — different log message.
- Thinking approve failed when broker is down — warning only; DB decision stands.
- Approve/reject slash in unquoted Mermaid nodes.

## Where it helps

### Health checklist

| Check | Good sign |
|---------|-------------|
| Consumers ≥ 1 | SMS consumer running |
| Approve + phone | Seq SMS line |
| Skip | Warning; flow 200 |

### Disaster / recovery (lab)

| Situation | Path |
|-------|-----|
| Queue backlog | `logs webapi`; restart consumer |
| No messages | Domain event? phone skip? |
