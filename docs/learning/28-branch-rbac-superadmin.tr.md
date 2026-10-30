# 28 — Şube RBAC + SuperAdmin

[← Önceki](27-branch-model.tr.md) · [İçindekiler](00-index.tr.md)

İlgili: [06 JWT / RBAC](06-jwt-rbac.tr.md) · [27 Şube modeli](27-branch-model.tr.md)

## Bu nedir?

**Şube kapsamlı yetkilendirme**: Personel yalnızca kendi şubesinin müşteri/başvurusunu görür ve değiştirir; yönetici diğer şubeleri **okuyabilir** ama yazamaz; **SuperAdmin** her şubede tam yetki + şube CRUD yapar.

## Bu projede ne işe yarar?

| Rol | Görüntüleme | Değiştirme (approve/update…) | Şube CRUD |
|-----|-------------|------------------------------|-----------|
| BankStaff | Kendi şube | Kendi şube | Yok |
| BankManager | Tüm şubeler (liste varsayılanı: kendi) | Kendi şube | Yok |
| SuperAdmin | Tümü + `?branchId=` | Tümü | Evet |
| Customer | Kendi kaydı | Kendi kaydı | Yok |

- `Customer.BranchId` / `CreditApplication.BranchId` (create’te müşteriden kopya).
- JWT `branchId` claim; `IBranchAuthorizationService`.
- Seed: NYC-01, CHI-01; `superadmin@bank.local`; pending başvurularda sahte evrak.

## Ne zaman eklendi?

Roadmap şube RBAC dilimi (2026-10-29 / 10-30).

## Temel kavramlar

| Kavram | Anlamı |
|--------|--------|
| **View vs modify** | Manager başka şubeyi görebilir; approve/reject edemez |
| **List filter** | Staff zorunlu kendi şube; Manager default kendi; SuperAdmin null = tümü |
| **SuperAdmin bypass** | `AuthorizationBehavior` tüm `ISecuredRequest` rollerini geçer |
| **AccessDenied audit** | Şube-cross red Seq’e yazılır |

## Komutlar

Rebuild + migrate + seed (wipe gerekmez; BranchId backfill seed ile gelir):

```powershell
cd infra
docker compose --env-file .env up -d --build webapi client
```

Doğrulama — SuperAdmin login + şube listesi:

```powershell
# Login
curl.exe -s -X POST http://localhost:8080/api/Auth/Login -H "Content-Type: application/json" -d "{\"email\":\"superadmin@bank.local\",\"password\":\"Demo123!\"}"
# Token ile: GET /api/Branches ; POST /api/Branches (CRUD)
```

Staff başka şube başvurusunu approve edemez → 403.

Lab şifresi (tüm demo hesaplar): `Demo123!` — Personel/Yönetici/Müşteri × `tr` / `de` / `en` e-posta sonekleri.

## Sınıflar, metotlar, paketler

- `GeneralOperationClaims.SuperAdmin`
- `IBranchAuthorizationService` / `BranchAuthorizationService`
- `ICurrentUserService.IsSuperAdmin` / `BranchId`
- `JwtHelper.CreateToken(..., branchId)`
- Branch CRUD: `CreateBranchRequest` / `UpdateBranchRequest`
- Seed: `BranchSeed`, `CustomerBranchAssignmentSeed`, `ApplicationBranchAssignmentSeed`, `CreditApplicationDocumentSeed`

## Repoda nerede?

- `src/Application/Services/Auth/BranchAuthorizationService.cs`
- `src/WebApi/Services/CurrentUserService.cs`
- `src/WebApi/Modules/Identity/Controllers/BranchesController.cs`
- `src/Persistence/Seed/*Branch*`
- `client/src/pages/BranchesPage.tsx`, `BranchFilter.tsx`
- Migration: `AddCustomerAndApplicationBranchId`

## Mini akış

1. `staff.nyc@` login → JWT’de NYC branchId → yalnızca NYC başvuruları.
2. `manager.nyc@` `?branchId=` ile CHI listesini okur → approve butonu UI’da gizli; API 403.
3. `superadmin@` Şubeler sayfasından CHI-01 günceller; herhangi bir başvuruyu onaylar.

## Where useful

- [ ] SuperAdmin login + `/branches` CRUD
- [ ] Staff NYC / CHI izolasyonu
- [ ] Manager cross-branch read-only
- [ ] Pending başvuruda demo PDF/JPEG görünür
- [ ] Seq’te `AccessDenied` (yanlış şube yazma denemesi)

Kurtarma (hafif → ağır): WebApi recreate → seed backfill; gerekirse volume wipe (veri kaybı).
