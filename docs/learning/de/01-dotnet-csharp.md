# 01 — .NET- und C#-Grundlagen

[← Inhalt](00-index.md) · [Weiter: Clean Architecture →](02-clean-architecture.md)

## Was ist das?

**.NET** ist Microsofts Plattform zum Ausführen von Anwendungen (Runtime + Bibliotheken + SDK).  
**C#** ist die Sprache, in der wir auf dieser Plattform schreiben. Das BankApp-Backend ist in **.NET 10** / **C#** geschrieben.

**Vergleich:** `BankApp.slnx` ist ein **Aktenschrank**; jede `.csproj` ist eine Schublade. `dotnet build` prüft alle Schubladen; `dotnet run` öffnet die WebApi-Schublade und lauscht auf Anfragen.

„SDK“ = das Toolset auf deinem Rechner, das den Befehl `dotnet` ausführt.

```mermaid
flowchart LR
  A[Quellcode] --> B[dotnet build]
  B --> C[Kestrel]
  C --> D[Client]
```

## Rolle in BankApp

- Die gesamte API (`WebApi`), die Geschäftslogik (`Application`), die Domäne (`Domain`) und der Datenzugriff (`Persistence`) sind C#-Projekte.
- Lösungsdatei: `BankApp.slnx` (verwaltet mehrere `.csproj`-Dateien gemeinsam).

## Wann hinzugefügt?

**Faz 1** — vom ersten Projekt tag an.

## Grundbegriffe

| Begriff | Bedeutung |
|--------|--------|
| **Solution (`.sln` / `.slnx`)** | „Lösung“, die mehrere Projekte gruppiert |
| **Project (`.csproj`)** | Eine kompilierbare Einheit (z. B. WebApi) |
| **Namespace** | Namensraum, der Klassen wie Ordner gruppiert |
| **Class / Interface** | Typdefinitionen; `interface` ist ein Vertrag, `class` die Implementierung |
| **async / await** | Warten bei I/O, ohne den Thread zu blockieren |
| **NuGet** | .NET-Paketmanager (`PackageReference`) |
| **TargetFramework** | Z. B. `net10.0` — welche .NET-Version zum Kompilieren verwendet wird |

```mermaid
sequenceDiagram
  participant Dev as Entwickler
  participant SDK as dotnet CLI
  participant Build as Build
  participant Host as Kestrel
  Dev->>SDK: dotnet build BankApp.slnx
  SDK->>Build: Alle csproj kompilieren
  Build-->>Dev: Erfolg oder Fehler
  Dev->>SDK: dotnet run --project WebApi
  SDK->>Host: API starten
  Host-->>Dev: Listening URL
```

## Befehle

```powershell
# SDK-Version anzeigen
dotnet --version

# Projekte in der Solution kompilieren
dotnet build BankApp.slnx

# Nur WebApi starten (Development)
dotnet run --project src/WebApi/WebApi.csproj

# NuGet-Pakete herunterladen (restore)
dotnet restore BankApp.slnx

# Tests ausführen
dotnet test BankApp.slnx --configuration Release
```

| Parameter / Flag | Zweck |
|--------------------|----------------|
| `--project` | Welche `.csproj` ausgeführt wird |
| `--configuration Release` | Optimiertes Produktions-Build (in CI) |
| `--no-restore` | Kein erneutes Restore, wenn CI Restore bereits separat ausführte |

## Klassen, Methoden, Pakete

- Beispiel-Projektdatei: `src/WebApi/WebApi.csproj` → `<TargetFramework>net10.0</TargetFramework>`
- Einstiegspunkt: `src/WebApi/Program.cs` — richtet den Host ein, registriert DI (Dependency Injection), fügt Middleware hinzu.
- Gemeinsame Infrastruktur: `src/Core` (Pipeline, Exception, Security-Hilfen).

Wichtige Paketideen für Einsteiger (Details in späteren Kapiteln):

- `MediatR` — Request-Routing  
- `FluentValidation` — Validierung  
- `Microsoft.EntityFrameworkCore.*` — Datenbank  

## Wo im Repo?

```
BankApp.slnx
src/WebApi/Program.cs
src/WebApi/WebApi.csproj
src/Application/Application.csproj
src/Domain/Domain.csproj
src/Persistence/Persistence.csproj
src/Core/Core.csproj
```

## Mini-Ablauf

1. `dotnet build` → C#-Quellen werden zu IL kompiliert.  
2. `dotnet run --project src/WebApi` → Der Kestrel-Webserver startet.  
3. Browser / React-Client sendet HTTP-Anfragen an die API unter `http://localhost:…`.

Bildschirm (Scalar API-Doku):

<img src="../../assets/screenshots/en/Api%20Documents%20with%20Scalar.png" alt="Scalar API-Dokumentation" width="720" />

## Einstiegs-Lab

1. `dotnet --version` — SDK installiert? (Projekt: `net10.0`).
2. Im Repo-Wurzelverzeichnis: `dotnet restore BankApp.slnx`.
3. `dotnet build BankApp.slnx` — „Build succeeded“.
4. `dotnet run --project src/WebApi/WebApi.csproj` — Listening-URL notieren.
5. `/health` oder Scalar im Browser — 200 / Doku lädt.

## Häufige Fehler

- Altes SDK → SDK aktualisieren oder `global.json` prüfen.  
- `dotnet run` im falschen Ordner → `--project` mit vollem Pfad.  
- Restore überspringen → „package not found“.  

## Wo hilft es?

- [ ] `dotnet build` für die ganze Solution grün  
- [ ] WebApi lokal läuft und health antwortet  
- [ ] Gleiche SDK-Version wie CI für `dotnet test`  
