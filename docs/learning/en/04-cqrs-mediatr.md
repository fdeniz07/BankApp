# 04 — CQRS and MediatR

[← Previous](03-ef-core-sql.md) · [Contents](00-index.md) · [Next →](05-validation-smartenum.md)

## What is this?

**CQRS** (Command Query Responsibility Segregation): the idea of separating **writes** (Command) from **reads** (Query).  
**MediatR**: Instead of the controller `new`-ing a handler directly, it hands the request to a mediator (“process this request”).

**Analogy:** MediatR is the bank **reception desk**; the customer (controller) asks for “approval”, the desk routes to the right clerk (handler) after security checks (pipeline).

```mermaid
flowchart LR
  C[Controller] --> M[MediatR]
  M --> V[Pipeline\nvalidation/auth]
  V --> H[IRequestHandler]
  H --> DB[(SQL / cache)]
```

## Role in BankApp

- `ApproveCreditApplicationRequest` → Handler approves.
- `GetListCreditApplicationRequest` → Handler lists.
- Pipeline: validation, authorization, logging, caching (`IPipelineBehavior`).

## When added?

**Phase 1**.

## Core concepts

| Concept | Meaning |
|--------|--------|
| **IRequest\<TResponse\>** | MediatR request contract |
| **IRequestHandler\<,\>** | Class that handles the request |
| **IPipelineBehavior** | Middleware that runs before/after the handler |
| **Command** | Request that changes state |
| **Query** | Read that does not change state |

```mermaid
sequenceDiagram
  participant C as Controller
  participant M as MediatR
  participant P as Pipeline
  participant H as Handler
  C->>M: Send request
  M->>P: Authorization
  P->>P: Validation
  P->>H: Handle
  H-->>C: Response
```

## Commands

Adding MediatR (already in the repo; for learning):

```powershell
dotnet add src/Application package MediatR
```

Not a runtime CLI; DI registration:

```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(...));
```

(Similar registration exists in `ApplicationServiceRegistration`.)

## Classes, methods, packages

| Name | Role |
|------|-----|
| `IMediator.Send(request)` | Sends the request to the handler |
| `ApproveCreditApplicationRequest` | Approve command |
| `ApproveCreditApplicationRequestHandler.Handle` | Main work |
| `RequestValidationBehavior` | Runs FluentValidation |
| `AuthorizationBehavior` | Checks `ISecuredRequest` roles |
| `LoggingBehavior` | Duration / error logging |

Package: `MediatR` (Application + Core pipeline).

## Where in the repo?

```
src/Application/Modules/**/Features/**/Commands/
src/Application/Modules/**/Features/**/Queries/
src/Core/Application/Pipelines/
src/WebApi/Modules/**/Controllers/
```

## Mini flow

`POST /api/CreditApplications/{id}/approve`  
→ Controller `mediator.Send(new Approve…Request { … })`  
→ Authorization → Validation → Logging → Handler  
→ Response DTO.

Screen (approve flow UI):

<img src="../../assets/screenshots/en/Bank%20Employee/Bank%20Employee%20Page%20-%20Approving-Rejecting%20Credit%201.png" alt="Approve UI" width="720" />

## Beginner lab

1. Open a controller under `src/WebApi/Modules` → confirm `IMediator` injection.
2. Find the matching handler under `Application/.../Commands`.
3. Trace `Send` → pipeline order (log or breakpoint).
4. Try a POST in Scalar → compare body with the handler.
5. Contrast a query handler (`Queries/`) with a command (no side effects).

## Common mistakes

- Business rules in controllers → move to handlers.  
- Calling `SaveChanges` in a query → command responsibility.  
- Adding a pipeline behavior without DI registration → it never runs.  

## Where it helps

- [ ] New endpoint = new Request + Handler pair  
- [ ] Validation/roles in one place (pipeline)  
- [ ] Controllers stay thin  
