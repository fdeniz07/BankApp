# 03 — EF Core ve SQL Server

[← Önceki](02-clean-architecture.tr.md) · [İçindekiler](00-index.tr.md) · [Sonraki →](04-cqrs-mediatr.tr.md)

## Bu nedir?

**Entity Framework Core (EF Core)**, C# nesnelerini SQL tablolarına map’leyen ORM’dir.  
**SQL Server**, ilişkisel veritabanıdır. BankApp’te ana kalıcılık buradadır.

## Bu projede ne işe yarar?

- Müşteri, kredi tipi, başvuru, outbox, saga satırları SQL’de tutulur.
- **TPT** (Table Per Type): bireysel / kurumsal müşteri ayrı tablolarda, ortak taban.
- Migration + seed (Bogus + sabit katalog) ile lab verisi üretilir.

## Ne zaman eklendi?

**Faz 1** (DB); Compose ile SQL **Faz 3**.

## Temel kavramlar

| Kavram | Anlamı |
|--------|--------|
| **DbContext** | Oturum + change tracking |
| **DbSet\<T\>** | Bir entity koleksiyonu (tablo gibi) |
| **Migration** | Şema değişikliğinin versiyonu |
| **Include** | İlişkili veriyi eager load |
| **Soft delete** | Satırı silmek yerine `DeletedDate` vb. işaretlemek |
| **TPT** | Kalıtım için ayrı tablolar |

## Komutlar

```powershell
# Migration ekle (Persistence projesinden / Design-time factory ile)
dotnet ef migrations add Isim --project src/Persistence --startup-project src/WebApi

# Veritabanını güncelle
dotnet ef database update --project src/Persistence --startup-project src/WebApi
```

| Bayrak | Anlamı |
|--------|--------|
| `--project` | Migration’ların bulunduğu proje |
| `--startup-project` | Connection string / DI’nın okunduğu host |

Docker lab’de port genelde `11433` → container içi `1433` (`infra/docker-compose.yml`).

## Sınıflar, metotlar, paketler

| Paket | Rol |
|-------|-----|
| `Microsoft.EntityFrameworkCore` | Temel API |
| `Microsoft.EntityFrameworkCore.SqlServer` | SQL provider |
| `Microsoft.EntityFrameworkCore.Design` / `Tools` | Migration CLI |

- Context: `Persistence/Contexts/BankingDbContext.cs`
- Config: `Persistence/EntityConfigurations/*`
- Repo: `Persistence/Repositories/*` implementing `Application/Services/Repositories/*`
- Önemli metotlar: `GetAsync`, `AddAsync`, `UpdateAsync`, `SaveChangesAsync`

## Repoda nerede?

```
src/Persistence/Contexts/BankingDbContext.cs
src/Persistence/EntityConfigurations/
src/Persistence/Repositories/
src/Domain/Entities/
infra/docker-compose.yml   # sqlserver servisi
```

## Mini akış

Handler `ICreditApplicationRepository.GetAsync(...)` çağırır → EF SQL üretir → satır belleğe gelir → `Approve(...)` → `UpdateAsync` → `SaveChangesAsync` transaction commit.
