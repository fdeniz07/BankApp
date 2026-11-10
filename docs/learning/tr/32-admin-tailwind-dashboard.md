# 32 — Admin karşılama dashboard (Tailwind + paket sıkıştırma)

[← Önceki](31-elk-elasticsearch.md) · [İçindekiler](00-index.md)

İlgili: [11 React + Vite](11-react-vite.md) · Dashboard API (Credit modül)

## Bu nedir?

**Tailwind CSS**, utility-first bir stil kütüphanesidir: sayfada hazır “kart / grid / spacing” sınıfları (`rounded-xl`, `grid`, `px-4` …) yazarsın; ayrı büyük CSS dosyası yerine build sırasında kullanılan sınıflar paketlenir.

**Paket sıkıştırma / chunking**, Vite üretim build’inde JS’i parçalara böler (react, router, i18n…) ve nginx’in servis edebileceği **`.gz`** dosyaları üretir — ilk yükleme daha küçük HTTP cevapları alır.

```mermaid
flowchart LR
  MGR[Manager / SuperAdmin] --> R[/dashboard]
  R --> TW[Tailwind KPI kartları]
  TW --> API[GET Dashboard/summary]
  VITE[Vite build] --> GZ[gzip_static nginx]
```

## Bu projede ne işe yarar?

- Manager / SuperAdmin girişte **`/dashboard`** görür (Tailwind ile KPI + hızlı linkler)
- Mevcut `GET /api/Dashboard/summary` yeniden kullanılır (yeni backend yok)
- Personel / müşteri hâlâ `/applications` karşılama paneli kullanır
- Compose client imajı: `manualChunks` + `gzip_static`

## Ne zaman eklendi?

Faz 6 — Manager ES facet paneli sonrasında (roadmap: Admin Tailwind dashboard).

## Temel kavramlar

| Kavram | Anlam |
|--------|--------|
| Utility class | Tek iş yapan sınıf (`mt-3`, `flex`, `bg-amber-500`) |
| Preflight | Tailwind’in tarayıcı reset’i — BankApp’te **kapalı** (eski `index.css` bozulmasın) |
| manualChunks | Vite/Rollup’ta vendor paketlerini ayrı JS dosyalarına ayırma |
| gzip_static | nginx, hazır `.gz` dosyasını sıkıştırma maliyeti olmadan gönderir |
| Admin home | Rol bazlı varsayılan rota (`homePath`) |

## Komutlar

### Bağımlılıkları kur (istemci)

TLS kurumsal proxy sorununda: `$env:NODE_OPTIONS='--use-system-ca'`

```powershell
cd client
npm install
```

Doğrula: `package.json` içinde `tailwindcss`, `@tailwindcss/vite`, `vite-plugin-compression`.

### Geliştirme sunucusu

```powershell
cd client
npm run dev
```

Tarayıcı: http://localhost:3000 — manager ile giriş → `/dashboard`.

### Üretim build + chunk / gzip kontrolü

```powershell
cd client
npm run build
```

Sonra: `dist/assets/` altında `react-vendor-*.js`, `i18n-*.js`, `*.gz` dosyaları olmalı.  
Eski tek ~500 KB bundle uyarısı chunk’larla bölünür.

### Compose client’ı yeniden build

```powershell
cd infra
docker compose --env-file .env up -d --build client
```

## Sınıflar, metotlar, paketler

- `AdminDashboardPage`, `HomeRedirect`, `homePath()`
- `getDashboardSummary` / `DashboardSummary`
- NuGet değil — npm: `tailwindcss`, `@tailwindcss/vite`, `vite-plugin-compression`
- Vite: `manualChunks` + compression plugin; nginx: `gzip_static on`

## Repoda nerede?

- `client/src/pages/AdminDashboardPage.tsx`
- `client/src/styles/tailwind.css`
- `client/src/utils/homePath.ts`
- `client/src/components/HomeRedirect.tsx`
- `client/vite.config.ts`, `client/nginx.conf`
- `client/src/App.tsx`, `AppLayout.tsx`, `LoginPage.tsx`

## Mini akış

1. Manager login → `homePath(true)` → `/dashboard`  
2. Sayfa `getDashboardSummary` çeker → KPI kartları  
3. “Bekleyen” linki → `/applications?status=Pending`  
4. `npm run build` → ayrı vendor chunk’lar + `.gz` → nginx `gzip_static`

## Nerede işe yarar?

- [ ] Manager girişinde `/dashboard` açılıyor  
- [ ] KPI sayıları StatsStrip / eski özet ile uyumlu  
- [ ] Staff / müşteri `/applications` karşılama panelini görüyor  
- [ ] `npm run build` sonrası `dist/assets/*.gz` var  
- [ ] Compose client healthy; manager UI Tailwind kartları gösteriyor  

### Felaket / kurtarma (az yıkıcı → çok)

1. `npm run build` lokal hata → `NODE_OPTIONS=--use-system-ca` ile `npm install`  
2. Stil kaybolmuş → `tailwind.css` import sırası (`main.tsx`: tailwind sonra `index.css`)  
3. Client imaj eski → `docker compose up -d --build client`  
