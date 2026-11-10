# 06 — JWT und RBAC

[← Zurück](05-validation-smartenum.md) · [Inhalt](00-index.md) · [Weiter →](07-serilog-seq.md)

## Was ist das?

**JWT** (JSON Web Token): Signiertes Identitätsticket, das der Client nach dem Login bei jeder Anfrage mitschickt.  
**RBAC** (Role-Based Access Control): Rechte nach **Rolle** (`Customer`, `BankStaff`, `BankManager`).

```mermaid
flowchart LR
  LOGIN[POST /Auth/login] --> JWT[JWT-Token]
  JWT --> HDR[Authorization Bearer]
  HDR --> API[Geschützte API]
  API --> RBAC[Rollen- / Claim-Prüfung]
```

## Rolle in BankApp

- `POST /api/Auth/login` → Access Token.
- Customer erstellt/storniert eigene Anträge; Staff genehmigt; Manager verwaltet Kredittypen.
- MediatR `ISecuredRequest` + `Roles`-Liste wird in der Pipeline geprüft.

## Wann hinzugefügt?

**Faz 2** (BankManager später erweitert).

## Grundbegriffe

| Begriff | Bedeutung |
|--------|--------|
| **Claim** | Kleine Information im Token (sub, role, email) |
| **Bearer** | Schema `Authorization: Bearer <token>` |
| **Access token** | API-Zugangsticket (kann kurzlebig sein) |
| **Password hash** | Passwort wird nicht als Klartext gespeichert |
| **ICurrentUserService** | Benutzerkontext aus dem Token |

## Befehle

JWT-Erzeugung nicht per CLI; zum Testen:

```powershell
# API läuft (Beispiel)
curl -X POST http://localhost:8080/api/Auth/login `
  -H "Content-Type: application/json" `
  -d "{\"email\":\"staff.tr@bank.local\",\"password\":\"Demo123!\"}"
```

Demo-Konten: Root `README.md` / `README.tr.md`.

## Klassen, Methoden, Pakete

| Name | Rolle |
|------|-----|
| `LoginRequestHandler` | Passwort prüfen, Token erzeugen |
| `ITokenHelper` / JWT-Einstellungen | Signatur + Claims |
| `ICurrentUserService` | `UserId`, `IsBankManager`, … |
| `AuthorizationBehavior` | Ohne Rolle → `AuthorizationException` |
| `GeneralOperationClaims` | Rollenkonstanten |

Pakete: `Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`.

## Wo im Repo?

```
src/Application/Modules/Identity/Features/Auth/
src/WebApi/Modules/Identity/
src/Core/Security/
src/Core/Application/Pipelines/Authorization/
```

## Mini-Ablauf

Login → Hash-Vergleich → Rollen in JWT-Claims → Client setzt Header → bei jedem MediatR-Command Rollenprüfung.
