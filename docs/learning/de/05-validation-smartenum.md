# 05 — FluentValidation und SmartEnum

[← Zurück](04-cqrs-mediatr.md) · [Inhalt](00-index.md) · [Weiter →](06-jwt-rbac.md)

## Was ist das?

**FluentValidation**: Flüssige Regeln für Request-Objekte (`RuleFor…`).  
**Ardalis SmartEnum**: Statt „magischer“ Strings/Ints typ-sichere enum-ähnliche Klassen mit Namen (`Pending`, `Approved` …).

**Vergleich:** FluentValidation ist die **Pförtnerkontrolle**; SmartEnum das offizielle **Statusetikett** auf der Karte.

```mermaid
flowchart LR
  REQ[Request] --> M[MediatR-Pipeline]
  M --> FV[FluentValidation]
  FV -->|gültig| H[Handler]
  FV -->|Fehler| ERR[400]
  SE[SmartEnum] --> REQ
```

## Rolle in BankApp

- Create-/Approve-/Reject-Validatoren lehnen falsche Beträge oder leeres Einkommen ab.
- `CreditApplicationStatus`, `CreditCategory`, `CreditRiskLevel` sind SmartEnums (in JSON meist **name**).

## Wann hinzugefügt?

**Faz 1**.

## Grundbegriffe

| Begriff | Bedeutung |
|--------|--------|
| **AbstractValidator\<T\>** | Regelsatz für T |
| **RuleFor** | Regel für eine Property |
| **When** | Bedingte Regel |
| **SmartEnum** | Klasse mit `Name` + `Value` |
| **TryFromName** | String → SmartEnum parsen |

```mermaid
stateDiagram-v2
  [*] --> Anfrage
  Anfrage --> Pruefung: Pipeline
  Pruefung --> Abgelehnt: Regel fail
  Pruefung --> Handler: Gültig
  Abgelehnt --> [*]: HTTP 400
  Handler --> [*]: Fertig
```

## Befehle

```powershell
dotnet add src/Application package FluentValidation.DependencyInjectionExtensions
dotnet add src/Domain package Ardalis.SmartEnum
```

## Klassen, Methoden, Pakete

| Beispiel | Rolle |
|-------|-----|
| `CreateCreditApplicationRequestValidator` | `MonthlyIncome` NotNull / GreaterThan(0) |
| `CreditApplicationStatus.Pending` | Statuskonstante |
| `CreditCategory.TryFromName(...)` | Filter parsen |

Pakete: `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`, `Ardalis.SmartEnum`, `Ardalis.SmartEnum.SystemTextJson` (WebApi).

## Wo im Repo?

```
src/Application/**/**RequestValidator.cs
src/Domain/Enums/
src/WebApi  # SmartEnum-JSON-Converter-Registrierung
```

## Mini-Ablauf

Anfrage kommt an → `RequestValidationBehavior` findet Validator → Regel scheitert → `ValidationException` → ProblemDetails 400.  
Bei Erfolg weiter zum Handler; Statusvergleich mit `CreditApplicationStatus.Pending`.

## Einstiegs-Lab

1. `CreateCreditApplicationRequestValidator` öffnen — `RuleFor` lesen.
2. Ungültiger POST (`monthlyIncome: 0`) → 400.
3. JSON-Status mit `CreditApplicationStatus.Pending.Name` vergleichen.
4. `TryFromName` mit unbekanntem String testen.
5. Gültige Anfrage → Handler, Status Pending.

## Häufige Fehler

- C#-`enum` mit SmartEnum mischen — anderes JSON.  
- Validator ohne `AbstractValidator` — Pipeline findet ihn nicht.  
- Magic String `"Pending"` — Konstante nutzen.  

## Wo hilft es?

- [ ] Schlechte Eingaben erreichen den Handler nicht  
- [ ] Statuswerte zentral in `Domain/Enums`  
- [ ] Feldfehler lesbar  
