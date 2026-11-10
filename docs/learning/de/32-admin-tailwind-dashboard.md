# 32 — Admin-Start-Dashboard (Tailwind + Bundle-Kompression)

[← Zurück](31-elk-elasticsearch.md) · [Inhalt](00-index.md) · [Weiter →](33-keycloak-lab.md)

Verwandt: [11 React + Vite](11-react-vite.md) · Dashboard-API (Credit-Modul)

## Was ist das?

**Tailwind CSS** ist eine Utility-first CSS-Bibliothek: fertige Klassen (`rounded-xl`, `grid`, `px-4` …); der Build packt nur **genutzte** Klassen.

**Chunking + gzip** teilt JS im Vite-Produktionsbuild (react, router, i18n …) und erzeugt **`.gz`**-Dateien für nginx — kleinere erste HTTP-Antworten.

**Vergleich:** Tailwind = LEGO-Steine; klassisches großes CSS = vorgebautes Modell. Chunk/gzip = Kiste aufteilen und komprimieren.

```mermaid
flowchart LR
  MGR["Manager / SuperAdmin"] --> DASH["Route /dashboard"]
  DASH --> TW["Tailwind KPI-Karten"]
  TW --> API["GET Dashboard/summary"]
  BUILD["Vite production build"] --> GZ["Chunk + gzip_static"]
```

## Rolle in BankApp

- Manager / SuperAdmin landen nach Login auf **`/dashboard`** (Tailwind-KPIs + Schnelllinks)
- Bestehendes `GET /api/Dashboard/summary` wird wiederverwendet (kein neues Backend)
- Personal / Kunde nutzen weiter `/applications` als Startpanel
- Compose-Client-Image: `manualChunks` + `gzip_static`

Bildschirm: [Bank Manager dashboard](../../screenshots/bank-manager.en.md) · [SuperAdmin-Galerie](../../screenshots/super-admin.en.md)

## Wann hinzugefügt?

Faz 6 — nach Manager-ES-Facet-Panel (Roadmap: Admin-Tailwind-Dashboard).

## Grundbegriffe

| Begriff | Bedeutung |
|---------|-----------|
| Utility class | Eine Aufgabe (`mt-3`, `flex`, `bg-amber-500`) |
| Preflight | Tailwind-Reset — in BankApp **aus** (bestehendes `index.css` bleibt) |
| manualChunks | Vendor-Pakete in separate JS-Dateien |
| gzip_static | nginx liefert vorgefertigte `.gz` ohne CPU-Last |
| Admin home | Rollenbasierte Standardroute (`homePath`) |

```mermaid
flowchart TB
  subgraph build["npm run build"]
    SRC["TSX + Tailwind"]
    CHUNK["manualChunks: react / router / i18n"]
    GZ[".js.gz Dateien"]
    SRC --> CHUNK --> GZ
  end
  subgraph runtime["Browser"]
    NGINX["nginx gzip_static"]
    PAGE["AdminDashboardPage"]
    NGINX --> PAGE
  end
  GZ --> NGINX
```

## Befehle

### Abhängigkeiten (Client)

Bei TLS/Proxy: `$env:NODE_OPTIONS='--use-system-ca'`

```powershell
cd client
npm install
```

Verifikation: `package.json` enthält `tailwindcss`, `@tailwindcss/vite`, `vite-plugin-compression`.

### Dev-Server

```powershell
cd client
npm run dev
```

Browser: http://localhost:3000 — Manager-Login → `/dashboard`.

### Produktionsbuild + Chunk/Gzip

```powershell
cd client
npm run build
```

Unter `dist/assets/`: `react-vendor-*.js`, `i18n-*.js`, `*.gz` sollten vorhanden sein.

### Compose-Client neu bauen

```powershell
cd infra
docker compose --env-file .env up -d --build client
```

## Klassen, Methoden, Pakete

- `AdminDashboardPage`, `HomeRedirect`, `homePath()`
- `getDashboardSummary` / `DashboardSummary`
- npm: `tailwindcss`, `@tailwindcss/vite`, `vite-plugin-compression`
- Vite: `manualChunks` + Compression-Plugin; nginx: `gzip_static on`

## Wo im Repo?

- `client/src/pages/AdminDashboardPage.tsx`
- `client/src/styles/tailwind.css`
- `client/src/utils/homePath.ts`
- `client/src/components/HomeRedirect.tsx`
- `client/vite.config.ts`, `client/nginx.conf`
- `client/src/App.tsx`, `AppLayout.tsx`, `LoginPage.tsx`

## Mini-Ablauf

1. Manager-Login → `homePath(true)` → `/dashboard`  
2. Seite lädt `getDashboardSummary` → KPI-Karten  
3. Link „Ausstehend“ → `/applications?status=Pending`  
4. `npm run build` → Vendor-Chunks + `.gz` → nginx `gzip_static`

Bildschirm (Manager-Dashboard):

<img src="../../assets/screenshots/en/Bank%20Manager/Bank%20Manager%20Page%20-%20Dashboard.png" alt="Manager-Dashboard" width="720" />

SuperAdmin-Dashboard:

<img src="../../assets/screenshots/en/Admin/Super%20Admin%20-%20Dashboard.png" alt="SuperAdmin-Dashboard" width="720" />

Kreditarten-Verwaltung (Manager):

<img src="../../assets/screenshots/en/Bank%20Manager/Bank%20Manager%20Page%20-%20Credit%20Types%20Management.png" alt="Kreditarten-Verwaltung" width="720" />

## Einstiegs-Lab

1. Mit `manager.*@bank.local` anmelden → Adresszeile `/dashboard`.
2. KPI-Zahlen grob mit StatsStrip vergleichen.
3. Mit Staff-Konto anmelden → Start unter applications, nicht `/dashboard`.
4. `npm run build` → `dist/assets/*.gz` prüfen.

## Häufige Fehler

- Preflight aktivieren → altes BankApp-CSS kaputt  
- Tailwind nicht **nach** `index.css` importieren → Stile weg  
- „Tailwind fehlt“ ohne Client-Image-Rebuild behaupten  

## Wo hilft es?

- [ ] Manager-Login öffnet `/dashboard`  
- [ ] KPI-Zahlen passen zur Zusammenfassung  
- [ ] Staff / Kunde sehen `/applications` als Start  
- [ ] Nach `npm run build` existieren `dist/assets/*.gz`  
- [ ] Compose-Client healthy; Tailwind-Karten sichtbar  

### Disaster / Recovery (weniger → mehr)

1. Lokaler Build-Fehler → `NODE_OPTIONS=--use-system-ca` und `npm install`  
2. Stile fehlen → `main.tsx`: zuerst `tailwind.css`, dann `index.css`  
3. Altes Client-Image → `docker compose up -d --build client`  
