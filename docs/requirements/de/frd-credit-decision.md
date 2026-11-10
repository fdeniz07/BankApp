# FRD — Kreditentscheidung: Dokumenten-Gate + Genehmigungsbefugnis

| Feld | Wert |
|------|------|
| Übergeordnetes BRD | [brd.md](brd.md) — BR-CR-07, BR-CR-08 |
| Module | Credit (Application / WebApi / client) |
| Letzte Aktualisierung | 2026-11-02 |

---

## 1. Zweck

Bei der Kreditgenehmigung durch Personal:

1. Pflichtdokumente (Ausweis + Einkommen) je Umgebung als **Soft** oder **Hard**  
2. **Positionsbasiertes** Genehmigungsbetragsmaximum (DoA)  

Beides wird per **appsettings** ein-/ausgeschaltet bzw. gewählt; Codepfade sind immer vorhanden.

---

## 2. Konfiguration

### 2.1 Dokumenten-Gate

```json
"CreditDocuments": {
  "ApproveGateMode": "Hard"
}
```

| Wert | UI | API Approve |
|--------|-----|-------------|
| `None` | Kein Soft-Confirm; kein gelbes Band | Kein Block |
| `Soft` | Confirm bei fehlenden Dokumenten + gelbes Band | Kein Block |
| `Hard` | Genehmigen-Button deaktiviert + rote Meldung | `MissingRequiredDocuments` |

Umgebungsempfehlung:

- `appsettings.Development.json` → `Soft`
- `appsettings.json` (Basis / prod-ähnlich) → `Hard`

Listen-Badge, Filter „Fehlende Pflichtdokumente“ und Preset „Ausstehend + fehlende Dokumente“ bleiben **in allen Modi**.

Pflichttypen: `IdentityDocument`, `IncomeProof` (`RequiredCreditDocuments`).

### 2.2 Genehmigungsbefugnis (DoA)

```json
"ApprovalAuthority": {
  "Enabled": true,
  "LimitsByPosition": {
    "CreditAnalyst": 50000,
    "RelationshipManager": 100000,
    "BranchOfficer": 75000,
    "RiskOfficer": 250000,
    "OperationsSpecialist": 25000,
    "BranchManager": 500000
  },
  "UnlimitedRoles": [ "BankManager", "SuperAdmin" ]
}
```

| Zustand | Verhalten |
|---------|-----------|
| `Enabled: false` | Kein Limit (Development-Standard) |
| Rolle in `UnlimitedRoles` | Kein Limit (`maxApprovalAmount: null`); Filialdeckel ebenfalls umgangen |
| In Positionstabelle | PositionMax = Wert |
| `Branch.MaxApprovalAmount` gesetzt | BranchMax = Wert; AppliedMax = `min(PositionMax, BranchMax)` |
| Filialdeckel null / ≤0 | Nur Positionsdeckel |
| Keine / undefinierte Position | PositionMax = 0 → Genehmigung praktisch blockiert |

Development: `Enabled: false`. Filialdeckel verwaltet SuperAdmin über `Branches` API/UI.

---

## 3. API

### 3.1 `GET /api/CreditDocuments/config`

Antwort (camelCase):

| Feld | Typ | Bedeutung |
|------|-----|-----------|
| `approveGateMode` | string | `None` \| `Soft` \| `Hard` |
| `softApproveGateEnabled` | bool | `mode == Soft` (abwärtskompatibel) |
| `approvalAuthorityEnabled` | bool | Options.Enabled |
| `maxApprovalAmount` | number \| null | AppliedMax; null = unbegrenzt / aus |
| `positionMaxApprovalAmount` | number \| null | Positionsdeckel (informativ) |
| `branchMaxApprovalAmount` | number \| null | Filialdeckel (informativ) |

### 3.2 `POST .../Approve` (bestehend)

Zusätzliche Prüfungen (der Reihe nach, nach bestehenden Regeln):

1. Hard + fehlende Pflichtdokumente → `BusinessException` / `MissingRequiredDocuments`
2. DoA an + Betrag > Deckel → `AmountExceedsApprovalAuthority`

---

## 4. UI (ApplicationDetailPage)

| Bedingung | Verhalten |
|-----------|-----------|
| Soft + fehlende Dokumente | Gelbe Warnung; extra Confirm bei Genehmigung |
| Hard + fehlende Dokumente | Rote Meldung; Submit deaktiviert |
| DoA an | Limit-Text + Position/Filiale → angewandte Aufschlüsselung; rot + Submit deaktiviert bei Überschreitung |
| API-Fehlermeldung | Bestehendes `mapApiError` / Toast |

---

## 5. Abnahmekriterien

- [ ] Development: Soft-Confirm funktioniert; Hard-Block nicht aktiv; hoher Betrag genehmigbar bei DoA aus  
- [ ] Basis-Config (oder Test-Override): Approve ohne Dokumente unter Hard liefert 400/Geschäftsregel  
- [ ] Approve abgelehnt bei Überschreitung CreditAnalyst-Limit; BankManager darf überschreiten  
- [ ] Approve abgelehnt nach Filialdeckel, wenn niedriger als Position  
- [ ] Unit: Hard fehlende Dokumente + Authority exceeded + Branch min  
- [ ] `GET .../config` liefert korrektes Modus/Limit/Aufschlüsselung für Personal-Session  

---

## 6. Wo im Repo?

- Options: `CreditDocumentsOptions`, `ApprovalAuthorityOptions`
- Evaluate: `ApprovalAuthorityEvaluator`
- Handler: `ApproveCreditApplicationRequestHandler`
- Entity: `Branch.MaxApprovalAmount`
- Controller: `CreditDocumentsController.GetConfig`
- Client: `creditDocumentsConfigApi.ts`, `ApplicationDetailPage.tsx`, `BranchesPage.tsx`
- Learning: [30-approve-gate-authority.md](../../learning/tr/30-approve-gate-authority.md)
