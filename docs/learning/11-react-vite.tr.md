# 11 — React, Vite ve TypeScript

[← Önceki](10-testing.tr.md) · [İçindekiler](00-index.tr.md) · [Sonraki →](12-i18n-ux.tr.md)

## Bu nedir?

**React**: Bileşen tabanlı UI kütüphanesi.  
**Vite**: Hızlı geliştirme sunucusu + production bundler.  
**TypeScript**: Tipli JavaScript — hataları derlemede yakalar.

## Bu projede ne işe yarar?

`client/` SPA: login, dashboard, başvurular, müşteriler, kredi türleri, personel, profil; API’ye JWT ile konuşur.

## Ne zaman eklendi?

**Faz 4**.

## Temel kavramlar

| Kavram | Anlamı |
|--------|--------|
| **Component** | `function Foo() { return <div/> }` |
| **Hook** | `useState`, `useEffect`, custom hook |
| **SPA** | Tek sayfa; client-side routing |
| **react-router** | URL → sayfa |
| **Fetch / api module** | HTTP istemci katmanı |

## Komutlar

```powershell
cd client
npm install          # package-lock’tan bağımlılık
npm run dev          # geliştirme sunucusu
npm run build        # tsc + vite production
npm run preview      # build çıktısını önizle
```

| Script | Ne yapar? |
|--------|-----------|
| `dev` | HMR ile local UI |
| `build` | `dist/` üretir (Docker nginx bunu sunar) |

Ortam: `VITE_API_BASE_URL` (Compose’ta build-arg).

## Sınıflar, metotlar, paketler

| Paket / dosya | Rol |
|---------------|-----|
| `react`, `react-dom` | UI |
| `react-router-dom` | Rotalar |
| `@microsoft/signalr` | Canlı bildirim |
| `client/src/api/*` | REST sarmalayıcılar |
| `AuthContext` | Token / kullanıcı state |
| `formatMoney` / `currencySymbol` | Para gösterimi |

## Repoda nerede?

```
client/package.json
client/src/main.tsx
client/src/pages/
client/src/api/
client/Dockerfile
```

## Mini akış

`npm run dev` → Login → token localStorage/context → ApplicationsPage list çeker → SignalR toast.
