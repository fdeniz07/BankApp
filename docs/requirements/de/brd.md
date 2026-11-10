# BRD — BankApp (Business Requirements Document)

| Feld | Wert |
|------|------|
| Produkt | BankApp — Schulungs- / Unternehmens-Lab-Bankanwendung |
| Status | Live (Lab); kontinuierliche Dokumentation |
| Sprache | Türkisch (primär) |
| Letzte Aktualisierung | 2026-11-02 |
| Verwandt | [roadmap](../../roadmap.de.md) · [history](../../history.de.md) · [FRD Kreditentscheidung](frd-credit-decision.md) |

---

## 1. Zweck und Umfang

### 1.1 Geschäftszweck

BankApp ist ein **Kreditantrag- und Entscheidungs**-Lab, das echte Bankprozesse nahe abbildet: Kundenantrag, Mitarbeiterprüfung, Filialbefugnis, Dokumente, Benachrichtigungen und Beobachtbarkeitsschichten in einem modularen Monolithen.

### 1.2 Im Umfang

- Privat- / Firmenkunden und Kreditproduktkatalog
- Antragslebenszyklus (anlegen / aktualisieren / stornieren / genehmigen / ablehnen)
- Mitarbeiter- und Manager-RBAC + Filialisolation
- Dokumenten-Upload und Genehmigungs-Gate (konfigurierbar Soft/Hard)
- Positionsbasierte Genehmigungsbefugnis (delegation of authority)
- Entscheidungsschreiben (Vorlage, E-Mail/SMS-Lab-Warteschlangen)
- React-Client (TR/EN/DE), Compose-Lab, CI

### 1.3 Außerhalb des Umfangs (vorerst / geplant)

- Elasticsearch-Mitarbeitersuche
- Produktions-Kubernetes / ArgoCD
- Raft Vault / vollständige Secret-Rotation
- OAuth / Keycloak
- KI-unterstützter Chat (Roadmap; später zu besprechen)
- Echte SMS-/E-Mail-Gateways

---

## 2. Stakeholder und Rollen

| Rolle | Geschäftsbedarf |
|-------|-----------------|
| Customer | Registrierung, E-Mail-Verifizierung, Antrag, Dokumente, Statusverfolgung, Storno |
| BankStaff | Prüfung im eigenen Filialbereich, Genehmigen/Ablehnen (Limit + Dokumentenregeln) |
| BankManager | Filialverwaltung, erweiterte Befugnis, Kreditartenverwaltung, Kundenregeln |
| SuperAdmin | Alle Filialen, Katalog-Locale-Freiheit, unbegrenzte Genehmigungsbefugnis (Config) |
| Lab-Dozent / Entwickler | Compose-Start, Seq/OTel, konfigurierbare Gates |

---

## 3. Geschäftsanforderungen (BR)

Status: **Erledigt** | **Teilweise** | **Geplant**

### 3.1 Identität und Zugriff

| ID | Anforderung | Status |
|----|-------------|--------|
| BR-ID-01 | Nutzer authentifizieren per JWT; Rollen Customer / BankStaff / BankManager / SuperAdmin | Erledigt |
| BR-ID-02 | Öffentliche Privatkunden-Registrierung; kein JWT bei Anmeldung; E-Mail-Verifizierung (Warteschlange + Log) | Erledigt |
| BR-ID-03 | Demo-Kontenmatrix (Personal/Manager/Kunde × TR/DE/EN); gemeinsames Lab-Passwort | Erledigt |
| BR-ID-04 | Datenschutz / Cookie-Einwilligung beim ersten Besuch | Erledigt |
| BR-ID-05 | Rate Limiting (auth/write/fixed) umgebungsabhängig | Erledigt |
| BR-ID-06 | Vault-Lab + JWT-Key-Overlay (Compose) | Erledigt |
| BR-ID-07 | OAuth / Social Login | Geplant |
| BR-ID-08 | Keycloak / Enterprise-IdP | Geplant |

### 3.2 Kunde und Produkt

| ID | Anforderung | Status |
|----|-------------|--------|
| BR-CU-01 | Privat- und Firmenkunden (TPT) | Erledigt |
| BR-CU-02 | Kredittyp / Untertyp; Locale-Katalog; Paket Betrag/Laufzeit-Raster | Erledigt |
| BR-CU-03 | Katalog an Filial-Locale gebunden (außer SuperAdmin) | Erledigt |
| BR-CU-04 | BankManager Kreditarten anlegen / aktiv-inaktiv | Erledigt |
| BR-CU-05 | Kundenlöschung: inaktiv + kein offener Kredit | Erledigt |

### 3.3 Kreditantrag und Entscheidung

| ID | Anforderung | Status |
|----|-------------|--------|
| BR-CR-01 | Kunde erstellt Antrag; Personal Genehmigen/Ablehnen/Storno-Regeln | Erledigt |
| BR-CR-02 | Tilgungsplan (Amortisation) + PDF-Export | Erledigt |
| BR-CR-03 | Listenfilter/Sortierung; fehlende-Dokumente-Badge und Preset | Erledigt |
| BR-CR-04 | Entscheidungs-Audit: wer, Filiale, Vorlagenname-Snapshot | Erledigt |
| BR-CR-05 | Benutzer-Entscheidungsschreiben-Vorlagen-CRUD + Systemvorlagen (UI-Sprache) | Erledigt |
| BR-CR-06 | Dokumenten-Upload (Ausweis, Einkommen usw.; PDF/JPEG/PNG) | Erledigt |
| BR-CR-07 | Genehmigungs-Dokumenten-Gate: None / Soft / Hard (Umgebungs-Config) | Erledigt |
| BR-CR-08 | Positionsbasiertes Genehmigungsbetragsmaximum; Manager-Rollen unbegrenzt (Config) | Erledigt |
| BR-CR-09 | Idempotente kritische Schreibvorgänge (Idempotency-Key) | Erledigt |

### 3.4 Filiale

| ID | Anforderung | Status |
|----|-------------|--------|
| BR-BR-01 | Filial-Entity; Personal/Kunde/Antrag BranchId | Erledigt |
| BR-BR-02 | Listen- und Änderungs-Filial-Guard; SuperAdmin alle Filialen | Erledigt |
| BR-BR-03 | Demo-Filialen (IST/ANK/NYC/CHI/BER usw.) | Erledigt |
| BR-BR-04 | Filial-Genehmigungsmaximum (Branch.MaxApprovalAmount); effektiv = min(Position, Filiale) | Erledigt |

### 3.5 Benachrichtigung und Echtzeit

| ID | Anforderung | Status |
|----|-------------|--------|
| BR-NT-01 | Domain Events + transaktionale Outbox → RabbitMQ | Erledigt |
| BR-NT-02 | SignalR Antrags-/Dokumenten-Events | Erledigt |
| BR-NT-03 | Entscheidungsschreiben-E-Mail-Warteschlange (Lab) | Erledigt |
| BR-NT-04 | Entscheidungsschreiben-SMS-Warteschlange (Lab; kein Gateway) | Erledigt |
| BR-NT-05 | Support-Chat-Bot | Geplant |

### 3.6 UX / Client

| ID | Anforderung | Status |
|----|-------------|--------|
| BR-UX-01 | React + Vite; TR/EN/DE i18n | Erledigt |
| BR-UX-02 | Rollenbasiertes Dashboard + Statistikleiste | Erledigt |
| BR-UX-03 | Profil / Avatar; Barrierefreiheit; Währungsleiste | Erledigt |
| BR-UX-04 | Admin-Willkommens-Dashboard (Tailwind / Performance) | Geplant |

### 3.7 Plattform / Qualität

| ID | Anforderung | Status |
|----|-------------|--------|
| BR-PL-01 | Clean Architecture + CQRS; Modular Monolith | Erledigt |
| BR-PL-02 | Docker Compose Lab-Stack | Erledigt |
| BR-PL-03 | Serilog+Seq; Redis; OTel→Grafana/Prometheus | Erledigt |
| BR-PL-04 | Unit- + Integrationstests; GitHub Actions CI | Erledigt |
| BR-PL-05 | Saga / Process Manager Kreditlebenszyklus | Erledigt |
| BR-PL-06 | Elasticsearch Personal/Suche (ELK-Lab + Live-Box; Seq bleibt) | Erledigt |
| BR-PL-06b | Manager/Admin erweitertes ES-Filterpanel | Erledigt |
| BR-PL-07 | Kubernetes / ArgoCD | Geplant |
| BR-PL-08 | Raft Vault / Secret-Rotation (Produktion) | Geplant |

### 3.8 Dokumentation

| ID | Anforderung | Status |
|----|-------------|--------|
| BR-DOC-01 | Learning-Bibliothek + Cursor-Regel | Erledigt |
| BR-DOC-02 | Roadmap (Kategorie) + History (Phase) | Erledigt |
| BR-DOC-03 | Dieses BRD + zugehörige FRDs (aktuell gehalten) | Erledigt |

---

## 4. Geschäftsregeln (Kurzüberblick)

1. Nur **Pending**-Anträge können genehmigt/abgelehnt/storniert werden.
2. Personal ändert nur Ressourcen der berechtigten **Filiale** (außer SuperAdmin).
3. Genehmigungsbetrag / Laufzeit müssen im **Produktpaket-Raster** liegen.
4. Bei **Hard**-Dokumenten-Gate keine Genehmigung ohne Ausweis + Einkommensnachweis.
5. Bei **Soft**-Gate Warnung; API kann dennoch genehmigen (Lab).
6. Bei aktivem **ApprovalAuthority** keine Genehmigung über Positionslimit; BankManager/SuperAdmin ausgenommen (Config).
7. Kritische Schreibvorgänge werden sicher mit **Idempotency-Key** wiederholt.

Detailliertes Verhalten: [frd-credit-decision.md](frd-credit-decision.md).

---

## 5. Erfolgskriterien (Lab)

- API + Client + Abhängigkeiten starten per Compose.
- Demo-Matrix schließt Personal-/Kundenabläufe in drei Sprachen ab.
- Soft-Gate + DoA aus in Development; Hard + DoA an in Basis-Config verifiziert.
- CI grün; Learning/BRD aktuell gehalten.

---

## 6. Annahmen und Risiken

| Annahme / Risiko | Hinweis |
|------------------|---------|
| Lab ≠ Produktionsbank | Kein Gateway, DSGVO-Prozesse, echtes Vault-HA |
| DoA-Limits PB-gekennzeichnet; bei nicht passendem PB nicht angewendet (kein FX) | Keine Cross-Currency- / Multi-PB-Umrechnung |
| Hard-Gate Pflichttypen fest | Identity + IncomeProof (`RequiredCreditDocuments`) |

---

## 7. Änderungsprotokoll (BRD)

| Datum | Änderung |
|-------|----------|
| 2026-11-01 | Erstes vollständiges BRD; BR-CR-07/08 (Dokumenten-Gate + DoA) ergänzt |
| 2026-11-02 | BR-BR-04 Filialdeckel abgeschlossen; effektiv = min(Position, Filiale) |
