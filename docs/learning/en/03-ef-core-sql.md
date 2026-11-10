# 03 — EF Core and SQL Server

[← Previous](02-clean-architecture.md) · [Contents](00-index.md) · [Next →](04-cqrs-mediatr.md)

## What is this?

**Entity Framework Core (EF Core)** is an ORM that maps C# objects to SQL tables.  
**SQL Server** is a relational database. In BankApp, main persistence lives here.

**Analogy:** EF Core is a **translator**; C# entities speak, SQL tables listen. Migrations are “dictionary updates” when the schema changes.

```mermaid
flowchart LR
  E[C# Entity] --> EF[EF Core DbContext]
  EF --> M[Migration]
  M --> SQL[(SQL Server)]
  EF -->|LINQ query| SQL
```

## Role in BankApp

- Customers, credit types, applications, outbox, and saga rows are stored in SQL.
- **TPT** (Table Per Type): individual / corporate customers in separate tables with a shared base.
- Migration + seed (Bogus + fixed catalog) produces lab data.

## When added?

**Phase 1** (DB); SQL via Compose in **Phase 3**.

## Core concepts

| Concept | Meaning |
|--------|--------|
| **DbContext** | Session + change tracking |
| **DbSet\<T\>** | A collection of one entity type (like a table) |
| **Migration** | A versioned schema change |
| **Include** | Eager load of related data |
| **Soft delete** | Mark rows with `DeletedDate` etc. instead of deleting |
| **TPT** | Separate tables for inheritance |

```mermaid
stateDiagram-v2
  [*] --> ModelChange
  ModelChange --> AddMigration: dotnet ef migrations add
  AddMigration --> UpdateDb: dotnet ef database update
  UpdateDb --> WorkingDb: Schema in sync
  WorkingDb --> ModelChange: New entity field
```

## Commands

```powershell
# Add migration (from Persistence project / design-time factory)
dotnet ef migrations add Name --project src/Persistence --startup-project src/WebApi

# Apply migrations to the database
dotnet ef database update --project src/Persistence --startup-project src/WebApi
```

| Flag | Meaning |
|--------|--------|
| `--project` | Project where migrations live |
| `--startup-project` | Host where connection string / DI is read |

In the Docker lab the port is usually `11433` → `1433` inside the container (`infra/docker-compose.yml`).

## Classes, methods, packages

| Package | Role |
|-------|-----|
| `Microsoft.EntityFrameworkCore` | Core API |
| `Microsoft.EntityFrameworkCore.SqlServer` | SQL provider |
| `Microsoft.EntityFrameworkCore.Design` / `Tools` | Migration CLI |

- Context: `Persistence/Contexts/BankingDbContext.cs`
- Config: `Persistence/EntityConfigurations/*`
- Repo: `Persistence/Repositories/*` implementing `Application/Services/Repositories/*`
- Important methods: `GetAsync`, `AddAsync`, `UpdateAsync`, `SaveChangesAsync`

## Where in the repo?

```
src/Persistence/Contexts/BankingDbContext.cs
src/Persistence/EntityConfigurations/
src/Persistence/Repositories/
src/Domain/Entities/
infra/docker-compose.yml   # sqlserver service
```

## Mini flow

Handler calls `ICreditApplicationRepository.GetAsync(...)` → EF generates SQL → row loads into memory → `Approve(...)` → `UpdateAsync` → `SaveChangesAsync` commits the transaction.

## Beginner lab

1. Ensure SQL is healthy in Compose (`infra`, `compose ps`).
2. Run `dotnet ef database update --project src/Persistence --startup-project src/WebApi`.
3. Connect with SSMS / Azure Data Studio to `localhost,11433` → inspect `CreditApplications`.
4. List an application in the UI → matches a DB row?
5. Find a `DbSet` in `BankingDbContext.cs` → read the entity under `Domain/Entities`.

## Common mistakes

- Omitting `--startup-project` → connection/DI not found.  
- Assuming port `1433` on the host → lab often uses `11433`.  
- Adding migrations but skipping `database update` → runtime “invalid column”.  

## Where it helps

- [ ] `database update` succeeds  
- [ ] Demo data after seed  
- [ ] Handlers use EF-generated SQL, not mandatory raw SQL  
