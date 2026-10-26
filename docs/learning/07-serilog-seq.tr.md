# 07 — Serilog ve Seq

[← Önceki](06-jwt-rbac.tr.md) · [İçindekiler](00-index.tr.md) · [Sonraki →](08-redis.tr.md)

## Bu nedir?

**Serilog**: Yapılandırılmış (.structured) log kütüphanesi.  
**Seq**: Bu logları toplayıp UI’da aramanı sağlayan sunucu (lab’de Docker).

## Bu projede ne işe yarar?

- İstek pipeline hataları, audit (`IAuditLogger` / `SerilogAuditLogger`), host yaşam döngüsü.
- OTel açıkken TraceId ile ilişkilendirme mümkün (log ↔ trace).

## Ne zaman eklendi?

**Faz 2**; Compose ile Seq **Faz 3**.

## Temel kavramlar

| Kavram | Anlamı |
|--------|--------|
| **Sink** | Log’un gittiği yer (konsol, Seq, dosya) |
| **Enricher** | Ortam adı, machine name gibi ek alan |
| **Structured log** | `"{UserId} login"` gibi property’li mesaj |
| **Audit** | “Kim ne yaptı?” iş olayı |

## Komutlar

```powershell
cd infra
docker compose up -d seq
# UI genelde http://localhost:5341 (compose’a bak)
```

Seq’te filtre örneği: `Application like '%BankApp%'` veya `RequestPath like '/api/%'`.

## Sınıflar, metotlar, paketler

| Paket / sınıf | Rol |
|---------------|-----|
| `Serilog.AspNetCore` | Host entegrasyonu |
| `Serilog.Sinks.Seq` | Seq sink |
| `UseSerilogRequestLogging` | HTTP istek logu |
| `IAuditLogger.Log(...)` | Auth / Credit audit kategorileri |

## Repoda nerede?

```
src/WebApi/Program.cs          # Serilog bootstrap
src/Application/Services/Logging/
infra/docker-compose.yml       # seq servisi
```

## Mini akış

Exception → LoggingBehavior / middleware → Serilog → Seq UI’da event listesi.
