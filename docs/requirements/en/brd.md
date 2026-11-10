# BRD — BankApp (Business Requirements Document)

| Field | Value |
|-------|--------|
| Product | BankApp — training / corporate lab banking application |
| Status | Live (lab); continuous documentation |
| Language | Turkish (primary) |
| Last updated | 2026-11-02 |
| Related | [roadmap](../../roadmap.en.md) · [history](../../history.en.md) · [FRD credit decision](frd-credit-decision.md) |

---

## 1. Purpose and scope

### 1.1 Business purpose

BankApp is a **credit application and decision** lab that closely mirrors real banking processes: customer application, staff review, branch authority, documents, notifications, and observability layers in a single modular monolith.

### 1.2 In scope

- Retail / corporate customers and credit product catalog
- Application lifecycle (create / update / cancel / approve / reject)
- Staff and manager RBAC + branch isolation
- Document upload and approval gate (configurable Soft/Hard)
- Position-based approval authority (delegation of authority)
- Decision letter (template, email/SMS lab queues)
- React client (TR/EN/DE), Compose lab, CI

### 1.3 Out of scope (for now / planned)

- Elasticsearch staff search
- Production Kubernetes / ArgoCD
- Raft Vault / full secret rotation
- OAuth / Keycloak
- AI-assisted chat (roadmap; to be discussed later)
- Real SMS/email gateways

---

## 2. Stakeholders and roles

| Role | Business need |
|------|----------------|
| Customer | Registration, email verification, application, documents, status tracking, cancellation |
| BankStaff | Review within own branch scope, approve/reject (subject to limit + document rules) |
| BankManager | Branch management, broader authority, credit type management, customer rules |
| SuperAdmin | All branches, catalog locale freedom, unlimited approval authority (config) |
| Lab instructor / developer | Compose bring-up, Seq/OTel, configurable gates |

---

## 3. Business requirements (BR)

Status: **Done** | **Partial** | **Planned**

### 3.1 Identity and access

| ID | Requirement | Status |
|----|-------------|--------|
| BR-ID-01 | Users authenticate with JWT; roles Customer / BankStaff / BankManager / SuperAdmin | Done |
| BR-ID-02 | Public retail registration; no JWT at signup; email verification (queue + log) | Done |
| BR-ID-03 | Demo account matrix (Staff/Manager/Customer × TR/DE/EN); shared lab password | Done |
| BR-ID-04 | Privacy / cookie consent on first visit | Done |
| BR-ID-05 | Rate limiting (auth/write/fixed) per environment | Done |
| BR-ID-06 | Vault lab + JWT key overlay (Compose) | Done |
| BR-ID-07 | OAuth / social sign-in | Planned |
| BR-ID-08 | Keycloak / enterprise IdP | Planned |

### 3.2 Customer and product

| ID | Requirement | Status |
|----|-------------|--------|
| BR-CU-01 | Retail and corporate customers (TPT) | Done |
| BR-CU-02 | Credit type / subtype; locale catalog; package amount/term grid | Done |
| BR-CU-03 | Catalog locked to branch locale (except SuperAdmin) | Done |
| BR-CU-04 | BankManager add credit types / active-inactive | Done |
| BR-CU-05 | Customer delete: inactive + no open credit | Done |

### 3.3 Credit application and decision

| ID | Requirement | Status |
|----|-------------|--------|
| BR-CR-01 | Customer creates application; staff approve/reject/cancel rules | Done |
| BR-CR-02 | Repayment schedule (amortization) + PDF export | Done |
| BR-CR-03 | List filter/sort; missing document badge and preset | Done |
| BR-CR-04 | Decision audit: who, branch, template name snapshot | Done |
| BR-CR-05 | User decision letter template CRUD + system templates (UI language) | Done |
| BR-CR-06 | Document upload (identity, income, etc.; PDF/JPEG/PNG) | Done |
| BR-CR-07 | Approval document gate: None / Soft / Hard (environment config) | Done |
| BR-CR-08 | Position-based approval amount ceiling; manager roles unlimited (config) | Done |
| BR-CR-09 | Idempotent critical writes (Idempotency-Key) | Done |

### 3.4 Branch

| ID | Requirement | Status |
|----|-------------|--------|
| BR-BR-01 | Branch entity; staff/customer/application BranchId | Done |
| BR-BR-02 | List and modify branch guard; SuperAdmin all branches | Done |
| BR-BR-03 | Demo branches (IST/ANK/NYC/CHI/BER, etc.) | Done |
| BR-BR-04 | Branch approval amount ceiling (Branch.MaxApprovalAmount); effective = min(position, branch) | Done |

### 3.5 Notification and real-time

| ID | Requirement | Status |
|----|-------------|--------|
| BR-NT-01 | Domain events + transactional outbox → RabbitMQ | Done |
| BR-NT-02 | SignalR application/document events | Done |
| BR-NT-03 | Decision letter email queue (lab) | Done |
| BR-NT-04 | Decision letter SMS queue (lab; no gateway) | Done |
| BR-NT-05 | Support chat bot | Planned |

### 3.6 UX / client

| ID | Requirement | Status |
|----|-------------|--------|
| BR-UX-01 | React + Vite; TR/EN/DE i18n | Done |
| BR-UX-02 | Role-based dashboard + statistics strip | Done |
| BR-UX-03 | Profile / avatar; accessibility; FX strip | Done |
| BR-UX-04 | Admin welcome dashboard (Tailwind / performance) | Planned |

### 3.7 Platform / quality

| ID | Requirement | Status |
|----|-------------|--------|
| BR-PL-01 | Clean Architecture + CQRS; Modular Monolith | Done |
| BR-PL-02 | Docker Compose lab stack | Done |
| BR-PL-03 | Serilog+Seq; Redis; OTel→Grafana/Prometheus | Done |
| BR-PL-04 | Unit + integration tests; GitHub Actions CI | Done |
| BR-PL-05 | Saga / process manager credit lifecycle | Done |
| BR-PL-06 | Elasticsearch staff/search (ELK lab + live box; Seq remains) | Done |
| BR-PL-06b | Manager/Admin advanced ES filter panel | Done |
| BR-PL-07 | Kubernetes / ArgoCD | Planned |
| BR-PL-08 | Raft Vault / secret rotation (production) | Planned |

### 3.8 Documentation

| ID | Requirement | Status |
|----|-------------|--------|
| BR-DOC-01 | Learning library + Cursor rule | Done |
| BR-DOC-02 | Roadmap (category) + history (phase) | Done |
| BR-DOC-03 | This BRD + related FRDs (kept current) | Done |

---

## 4. Business rules (summary)

1. Only **Pending** applications can be approved/rejected/cancelled.
2. Staff may only modify resources in their authorized **branch** (except SuperAdmin).
3. Approval amount / term must be within the **product package grid**.
4. Under **Hard** document gate, no approval without identity + income proof.
5. **Soft** gate shows a warning; API may still approve (lab).
6. When **ApprovalAuthority** is on, exceeding position limit cannot be approved; BankManager/SuperAdmin exempt (config).
7. Critical writes are safely retried with **Idempotency-Key**.

Detailed behaviour: [frd-credit-decision.md](frd-credit-decision.md).

---

## 5. Success criteria (lab)

- API + client + dependencies come up via Compose.
- Demo matrix completes staff/customer flows in three languages.
- Soft gate + DoA off in Development; Hard + DoA on in base config verified.
- CI green; learning/BRD kept current.

---

## 6. Assumptions and risks

| Assumption / risk | Note |
|-------------------|------|
| Lab ≠ production bank | No gateway, GDPR processes, real Vault HA |
| DoA limits PB-labelled; not applied on mismatched PB (no FX) | No cross-currency / multi-PB conversion |
| Hard gate mandatory types fixed | Identity + IncomeProof (`RequiredCreditDocuments`) |

---

## 7. Change log (BRD)

| Date | Change |
|------|--------|
| 2026-11-01 | First full BRD; BR-CR-07/08 (document gate + DoA) added |
| 2026-11-02 | BR-BR-04 branch ceiling completed; effective = min(position, branch) |
