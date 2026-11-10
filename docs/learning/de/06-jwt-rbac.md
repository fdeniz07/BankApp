# 06 — JWT und RBAC

[← Zurück](05-validation-smartenum.md) · [Inhalt](00-index.md) · [Weiter →](07-serilog-seq.md)

## Was ist das?

**JWT** (JSON Web Token): Signiertes Identitätsticket, das der Client nach dem Login bei jeder Anfrage mitschickt.  
**RBAC** (Role-Based Access Control): Rechte nach **Rolle** (`Customer`, `BankStaff`, `BankManager`).

**Vergleich:** JWT ist das **Festival-Armband**; der Türsteher (API) liest die Rolle und lässt zur Bühne (Endpoint) oder nicht.

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

```mermaid
sequenceDiagram
  participant U as Nutzer
  participant API as Auth API
  participant C as Client
  participant P as Geschützter Endpoint
  U->>API: E-Mail + Passwort
  API-->>U: JWT Access Token
  U->>C: Token speichern
  C->>P: Authorization Bearer
  P-->>C: 200 oder 403
```

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

Screenshot (Login): [`Login Page and Privacy.png`](../../assets/screenshots/en/Login%20Page%20and%20Privacy.png)

## Einstiegs-Lab

1. Demo-Konto aus README → `POST /api/Auth/login`.
2. Token auf jwt.io dekodieren — `role`-Claims (nicht committen).
3. Genehmigung mit Staff-Token → 200.
4. Gleicher Call mit Customer → 403.
5. Client-Login → Network: `Bearer` bei Folgerequests.

## Häufige Fehler

- Token in Query/Logs — nur Header.  
- Abgelaufenes Token — erneut einloggen.  
- Tippfehler bei Rollennamen — `GeneralOperationClaims`.  

## Wo hilft es?

- [ ] Login liefert Token  
- [ ] Falsche Rolle → 403  
- [ ] `ICurrentUserService` korrekt  
