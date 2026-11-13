# BankApp öğrenme kitaplığı (TR)

Sıfırdan teknoloji rehberi — **bu projede gerçekten kullanılan** stack’ler, **inşa sırasına** göre.

[tech-learning özeti](../../tech-learning.tr.md) · [tarihçe](../../history.tr.md) · [yol haritası](../../roadmap.tr.md) · [mimari notlar](../../architecture-notes.tr.md)

Ayrıca: [English](../en/00-index.md) · [Deutsch](../de/00-index.md)

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
| 1 | .NET / C# temelleri | [01](01-dotnet-csharp.md) |
| 1 | Clean Architecture | [02](02-clean-architecture.md) |
| 1 | EF Core + SQL Server | [03](03-ef-core-sql.md) |
| 1 | CQRS + MediatR | [04](04-cqrs-mediatr.md) |
| 1 | FluentValidation + SmartEnum | [05](05-validation-smartenum.md) |
| 2 | JWT + RBAC | [06](06-jwt-rbac.md) |
| 2 | Serilog + Seq | [07](07-serilog-seq.md) |
| 2–3 | Redis | [08](08-redis.md) |
| 3 | Docker Compose | [09](09-docker-compose.md) |
| 3–5 | xUnit / entegrasyon testleri | [10](10-testing.md) |
| 4 | React + Vite + TypeScript | [11](11-react-vite.md) |
| 4–5 | i18n + UX (erişilebilirlik vb.) | [12](12-i18n-ux.md) |
| 5 | RabbitMQ (e-posta doğrulama) | [13](13-rabbitmq.md) |
| 6 | Domain Events + Outbox | [14](14-outbox-events.md) |
| 6 | Idempotency-Key | [08](08-redis.md) (Redis bölümünde) + [14](14-outbox-events.md) |
| 6 | Saga / Process Manager | [15](15-saga.md) |
| 6 | SignalR | [16](16-signalr.md) |
| 6 | Modular Monolith | [17](17-modular-monolith.md) |
| 6 | OpenTelemetry / Grafana / Prometheus | [18](18-otel-observability.md) |
| Lab | HashiCorp Vault (AppRole, split KV, `cache`, FailClosed; JWT dual-key; opsiyonel Raft) | [19](19-vault-lab.md) |
| Lab / Security | Redis `requirepass` + Vault cache; günlük `lab-daily-check.ps1` | [08](08-redis.md) · [24](24-lab-tools.md) |
| UX | QuestPDF / ödeme planı export | [20](20-pdf-export.md) |
| DevOps | GitHub Actions CI | [21](21-ci-github-actions.md) |
| Notification | Karar mektubu e-posta (lab) | [22](22-decision-letter-email.md) |
| Security | Rate limiting (auth / write / fixed; ortam ayrımı) | [23](23-rate-limiting.md) |
| Lab / DX | İzleme araçları (Scalar, Seq, Insight, Grafana…) | [24](24-lab-tools.md) |
| Notification | Karar mektubu SMS (lab) | [25](25-decision-letter-sms.md) |
| Credit / UX | Başvuru evrak yükleme (lab) | [26](26-credit-application-documents.md) |
| Domain | Şube / branch modeli | [27](27-branch-model.md) |
| Security | Şube RBAC + SuperAdmin | [28](28-branch-rbac-superadmin.md) |
| Credit / UX | Karar mektubu şablon CRUD | [29](29-decision-letter-templates.md) |
| Credit / Security | Onay evrak kapısı + DoA (şube/pozisyon PB etiketli) | [30](30-approve-gate-authority.md) |
| Search / Observability | ELK + başvuru/personel/müşteri/kredi türü arama + facets | [31](31-elk-elasticsearch.md) |
| UX / DevOps | Admin Tailwind dashboard + Vite chunk/gzip | [32](32-admin-tailwind-dashboard.md) |
| Security / Lab | Keycloak (Compose + dual JWT + SPA PKCE + sosyal IdP) | [33](33-keycloak-lab.md) |
| UX / Lab | Taşınabilir erişilebilirlik kiti (a11y asistan) | [34](34-a11y-portable-kit.md) |

Gereksinim (BRD/FRD): [requirements/tr](../../requirements/tr/00-index.md)

Kitaplık güncelleme günlüğü: [99-changelog-learning.md](99-changelog-learning.md)

---

## Hızlı komut hatırlatıcıları

```powershell
# Çözümü derle
dotnet build BankApp.slnx

# Unit + integration test
dotnet test BankApp.slnx

# Günlük lab smoke (stack ayaktayken, repo kökü)
powershell -NoProfile -File infra/scripts/lab-daily-check.ps1

# Compose lab (infra klasöründen)
cd infra
docker compose --env-file .env up -d --build

# React istemci (client klasöründen)
cd client
npm install
npm run dev
```

Ayrıntılar ilgili bölümde.
