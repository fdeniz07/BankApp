# 05 — FluentValidation ve SmartEnum

[← Önceki](04-cqrs-mediatr.tr.md) · [İçindekiler](00-index.tr.md) · [Sonraki →](06-jwt-rbac.tr.md)

## Bu nedir?

**FluentValidation**: Request nesneleri için akıcı kurallar (`RuleFor…`).  
**Ardalis SmartEnum**: “sihirli string/int” yerine isimli, tip güvenli enum-benzeri sınıflar (`Pending`, `Approved`…).

## Bu projede ne işe yarar?

- Create/approve/reject validator’ları hatalı tutarı veya boş geliri reddeder.
- `CreditApplicationStatus`, `CreditCategory`, `CreditRiskLevel` SmartEnum’dır (JSON’da genelde **name**).

## Ne zaman eklendi?

**Faz 1**.

## Temel kavramlar

| Kavram | Anlamı |
|--------|--------|
| **AbstractValidator\<T\>** | T için kural seti |
| **RuleFor** | Bir property kuralı |
| **When** | Koşullu kural |
| **SmartEnum** | `Name` + `Value` taşıyan sınıf |
| **TryFromName** | String → SmartEnum parse |

## Komutlar

```powershell
dotnet add src/Application package FluentValidation.DependencyInjectionExtensions
dotnet add src/Domain package Ardalis.SmartEnum
```

## Sınıflar, metotlar, paketler

| Örnek | Rol |
|-------|-----|
| `CreateCreditApplicationRequestValidator` | `MonthlyIncome` NotNull / GreaterThan(0) |
| `CreditApplicationStatus.Pending` | Durum sabiti |
| `CreditCategory.TryFromName(...)` | Filtre parse |

Paketler: `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`, `Ardalis.SmartEnum`, `Ardalis.SmartEnum.SystemTextJson` (WebApi).

## Repoda nerede?

```
src/Application/**/**RequestValidator.cs
src/Domain/Enums/
src/WebApi  # SmartEnum JSON converter kaydı
```

## Mini akış

İstek gelir → `RequestValidationBehavior` validator bulur → kural fail → `ValidationException` → ProblemDetails 400.  
Başarılıysa handler’a geçilir; status karşılaştırması `CreditApplicationStatus.Pending` ile yapılır.
