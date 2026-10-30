# Roadmap

[Türkçe](roadmap.tr.md) · [English](roadmap.en.md)

Chronologische Phasen: [history.de.md](history.de.md). Architekturdiskussion: [architecture-notes.de.md](architecture-notes.de.md).

## Erledigt

### Architektur / Infrastruktur

- [x] Clean Architecture + CQRS (MediatR) .NET 10 API
- [x] Docker Compose (SQL, Redis, Seq, RabbitMQ, WebApi, React-Client)
- [x] Modular Monolith (Identity / Customers / Credit / Notification)
- [x] Domain Events + Transactional Outbox (Kreditanträge; Queue `domain-events`)
- [x] Idempotency-Key (Kredit create/approve/reject/cancel; Redis)
- [x] Saga / Process Manager (Kreditantrags-Lebenszyklus; `CreditApplicationSagas`)
- [x] SignalR-Benachrichtigungen (Outbox → Hub `/hubs/credit-applications`)

### Sicherheit / Identität

- [x] JWT-Auth; Customer / BankStaff / BankManager / SuperAdmin RBAC
- [x] Filial-RBAC (Listenfilter, Modify-Guard; NYC/CHI-Seed, Filial-CRUD)
- [x] E-Mail-Verifizierung nach Registrierung (RabbitMQ + Seq/Log; kein JWT bei Register)
- [x] Datenschutz- / Cookie-Einwilligung (Banner + gespeicherte Wahl)
- [x] Rate-Limit-Policies (auth / write / fixed; 429) + Umgebungs-Split (streng base, locker Development)
- [x] HashiCorp Vault-Lab (Compose-Dev-Modus + Seed `secret/bankapp`)
- [x] Vault → WebApi JWT-Key-Overlay (Lab; SQL weiter Compose-Env)
- [x] Demo-Login-Matrix Mitarbeiter/Manager/Kunde × TR/DE/EN; gemeinsames Lab-Passwort `Demo123!`

### Domain / Fachlogik

- [x] Privat- / Firmenkunden (TPT), Kreditarten / Untertypen / Anträge
- [x] SmartEnum für Status und Kategorien
- [x] Antrag stornieren, genehmigen / ablehnen, Zahlungsplan (Amortisation)
- [x] Locale-Katalog (TR/EN/DE) + Paket-Betrag/Laufzeit; Katalog an Filial-Locale gebunden (außer SuperAdmin)
- [x] Filial-/Branch-Modell + Entscheidungs-Snapshot (`DecisionBy` / Filiale / Vorlagenname)
- [x] Dokumenten-Upload beim Kreditantrag (Lab: PDF/JPEG/PNG, App_Data)
- [x] Soft-Genehmigungs-Gate (`SoftApproveGateEnabled`; in Development aus) + fehlende-Unterlagen-Badge/Filter/Listen-Preset
- [x] Entscheidungsbrief-Vorlagen-CRUD (Benutzer); Systemvorlagen folgen der UI-Sprache
- [x] BankManager: Kreditarten anlegen / aktivieren-deaktivieren; Kunden-Löschregeln

### UX / Client

- [x] React-Client (Vite + TypeScript), TR / EN / DE i18n
- [x] Währungsanzeige (TRY / EUR / USD), Kundenpanel, Formular neuer Antrag
- [x] Listensortierung / Statusfilter; Inline Genehmigen / Ablehnen für Mitarbeiter
- [x] Rollenbasiertes Dashboard + Statistikleiste
- [x] Personalverwaltung-UI; Mitarbeiter-/Kundenprofil + Avatar-Crop + Header-Avatar
- [x] Barrierefreiheits-Panel (Thema / Schriftgröße / Kontrast) + Wechselkurs-Ticker
- [x] Gender-Allow-List; öffentliche Privatkunden-Registrierung; Mitarbeiter-Kunden-UI

### Beobachtbarkeit / Qualität

- [x] Serilog + Seq; Redis-Cache
- [x] OpenTelemetry + Grafana / Prometheus (Collector + Tempo; Seq bleibt für Logs)
- [x] Application-Unit-Tests
- [x] WebApi-Integrationstests (Login-Smoke; create→approve; Ablehnen/Storno; Kundenregeln)
- [x] CI (GitHub Actions): Build + Tests + Client-Build (`ubuntu-24.04`, Actions v5)

### Benachrichtigungskanäle (Lab)

- [x] Entscheidungsbrief per E-Mail (Queue `decision-letter` + Seq-Log)
- [x] Entscheidungsbrief per SMS (Queue `decision-letter-sms` + Seq-Log; kein Gateway)

### Dokumentation

- [x] Einsteiger-Tech-Lernbibliothek (`docs/learning/`) + Cursor-Regel `tech-learning-library`
- [x] Roadmap / Historie / Architektur-Notizen (TR / EN / DE)

## Offen

- [ ] Elasticsearch für Mitarbeiter-GET-Endpunkte
- [ ] Admin-Welcome-Dashboard (Tailwind CSS + Bundle-Kompression / Performance)
- [ ] Kubernetes / ArgoCD Deployment
- [ ] Raft-Vault / Secret-Rotation (Produktion; jenseits Compose-Dev-Mode — Learning 19)
- [ ] OAuth (Registrierung / Login über soziale Netzwerke)
- [ ] Keycloak-Unterstützung
- [ ] Chatbot (Kunden- / Mitarbeiter-Support-Assistent)
