# BankApp öğrenme kitaplığı (TR)

Sıfırdan teknoloji rehberi — **bu projede gerçekten kullanılan** stack’ler, **inşa sırasına** göre.

[tech-learning özeti](../tech-learning.tr.md) · [tarihçe](../history.tr.md) · [yol haritası](../roadmap.tr.md) · [mimari notlar](../architecture-notes.tr.md)

---

## Bu kitaplık ne?

| | |
|--|--|
| **Kim için?** | Konuyu ilk kez gören / “repoda şu paket var, ne?” diyen herkes |
| **Ne içerir?** | Tanım, BankApp’teki rolü, Faz sırası, kavramlar, CLI, sınıflar/metotlar/paketler, dosya yolları, mini akış |
| **Ne değildir?** | Resmi dokümantasyonun kopyası; her satır kodun dökümü |
| **Güncelleme** | Her yeni özellikten sonra (Cursor kuralı: `tech-learning-library`; yerelde `.cursor/rules/`) |

---

## Nasıl okunmalı?

1. Aşağıdaki **Faz sırasını** takip et (atlama yapma; her bölüm öncekine dayanır).
2. Bir bölümü bitirince listedeki **Repoda nerede?** yollarını IDE’de aç.
3. Mümkünse küçük bir komutu kendin çalıştır (`dotnet build`, `docker compose ps`, …).
4. Ürün “neden böyle?” sorusu için `architecture-notes` / `tech-learning` özetine bak.

---

## Faz → bölüm haritası

| Faz | Konu | Bölüm |
|-----|------|--------|
| 1 | .NET / C# temelleri | [01](01-dotnet-csharp.tr.md) |
| 1 | Clean Architecture | [02](02-clean-architecture.tr.md) |
| 1 | EF Core + SQL Server | [03](03-ef-core-sql.tr.md) |
| 1 | CQRS + MediatR | [04](04-cqrs-mediatr.tr.md) |
| 1 | FluentValidation + SmartEnum | [05](05-validation-smartenum.tr.md) |
| 2 | JWT + RBAC | [06](06-jwt-rbac.tr.md) |
| 2 | Serilog + Seq | [07](07-serilog-seq.tr.md) |
| 2–3 | Redis | [08](08-redis.tr.md) |
| 3 | Docker Compose | [09](09-docker-compose.tr.md) |
| 3–5 | xUnit / entegrasyon testleri | [10](10-testing.tr.md) |
| 4 | React + Vite + TypeScript | [11](11-react-vite.tr.md) |
| 4–5 | i18n + UX (erişilebilirlik vb.) | [12](12-i18n-ux.tr.md) |
| 5 | RabbitMQ (e-posta doğrulama) | [13](13-rabbitmq.tr.md) |
| 6 | Domain Events + Outbox | [14](14-outbox-events.tr.md) |
| 6 | Idempotency-Key | [08](08-redis.tr.md) (Redis bölümünde) + [14](14-outbox-events.tr.md) |
| 6 | Saga / Process Manager | [15](15-saga.tr.md) |
| 6 | SignalR | [16](16-signalr.tr.md) |
| 6 | Modular Monolith | [17](17-modular-monolith.tr.md) |
| 6 | OpenTelemetry / Grafana / Prometheus | [18](18-otel-observability.tr.md) |
| Lab | HashiCorp Vault (+ WebApi JWT overlay) | [19](19-vault-lab.tr.md) |
| UX | QuestPDF / ödeme planı export | [20](20-pdf-export.tr.md) |
| DevOps | GitHub Actions CI | [21](21-ci-github-actions.tr.md) |
| Notification | Karar mektubu e-posta (lab) | [22](22-decision-letter-email.tr.md) |
| Security | Rate limiting (auth / write / fixed) | [23](23-rate-limiting.tr.md) |
| Lab / DX | İzleme araçları (Scalar, Seq, Insight, Grafana…) | [24](24-lab-tools.tr.md) |

Kitaplık güncelleme günlüğü: [99-changelog-learning.tr.md](99-changelog-learning.tr.md)

---

## Hızlı komut hatırlatıcıları

```powershell
# Çözümü derle
dotnet build BankApp.slnx

# Unit + integration test
dotnet test BankApp.slnx

# Compose lab (infra klasöründen)
cd infra
docker compose up -d --build

# React istemci (client klasöründen)
cd client
npm install
npm run dev
```

Ayrıntılar ilgili bölümde.
