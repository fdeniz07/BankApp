# 05 — FluentValidation and SmartEnum

[← Previous](04-cqrs-mediatr.md) · [Contents](00-index.md) · [Next →](06-jwt-rbac.md)

## What is this?

**FluentValidation**: Fluent rules for request objects (`RuleFor…`).  
**Ardalis SmartEnum**: Named, type-safe enum-like classes instead of magic strings/ints (`Pending`, `Approved`…).

```mermaid
flowchart LR
  REQ[Request] --> M[MediatR pipeline]
  M --> FV[FluentValidation]
  FV -->|valid| H[Handler]
  FV -->|error| ERR[400]
  SE[SmartEnum] --> REQ
```

## Role in BankApp

- Create/approve/reject validators reject invalid amounts or empty income.
- `CreditApplicationStatus`, `CreditCategory`, `CreditRiskLevel` are SmartEnums (JSON usually uses **name**).

## When added?

**Phase 1**.

## Core concepts

| Concept | Meaning |
|--------|--------|
| **AbstractValidator\<T\>** | Rule set for T |
| **RuleFor** | Rule for one property |
| **When** | Conditional rule |
| **SmartEnum** | Class carrying `Name` + `Value` |
| **TryFromName** | Parse string → SmartEnum |

## Commands

```powershell
dotnet add src/Application package FluentValidation.DependencyInjectionExtensions
dotnet add src/Domain package Ardalis.SmartEnum
```

## Classes, methods, packages

| Example | Role |
|-------|-----|
| `CreateCreditApplicationRequestValidator` | `MonthlyIncome` NotNull / GreaterThan(0) |
| `CreditApplicationStatus.Pending` | Status constant |
| `CreditCategory.TryFromName(...)` | Filter parse |

Packages: `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`, `Ardalis.SmartEnum`, `Ardalis.SmartEnum.SystemTextJson` (WebApi).

## Where in the repo?

```
src/Application/**/**RequestValidator.cs
src/Domain/Enums/
src/WebApi  # SmartEnum JSON converter registration
```

## Mini flow

Request arrives → `RequestValidationBehavior` finds validator → rule fails → `ValidationException` → ProblemDetails 400.  
On success, flow continues to the handler; status is compared with `CreditApplicationStatus.Pending`.
