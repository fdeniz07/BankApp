# Roadmap

[Türkçe](roadmap.tr.md) · [English](roadmap.en.md)

## Erledigt

- [x] Clean Architecture + CQRS (MediatR) .NET 10 API
- [x] Privat- / Firmenkunden (TPT), Kreditarten / Untertypen / Anträge
- [x] SmartEnum für Status und Kategorien
- [x] JWT-Auth, Customer / BankStaff RBAC
- [x] Serilog + Seq, Redis-Cache
- [x] Docker Compose (SQL, Redis, Seq, WebApi, React-Client)
- [x] Unit-Tests (Application)
- [x] React-Client (Vite + TypeScript), TR / EN / DE i18n
- [x] Währungsanzeige (TRY / EUR / USD), Kunden-Dashboard
- [x] Antrag stornieren (Cancel, kein Soft-Delete)
- [x] Listensortierung / Statusfilter, Kreditart-Spalte
- [x] Formular für neuen Kundenantrag
- [x] Sprachgerechte Seed-Namen, runde Beträge, Standardlaufzeiten
- [x] Locale-Katalog (TR/EN/DE) + Mitarbeiterfilter + Paket-Betrag/Laufzeit
- [x] Zahlungsplan (Amortisation)
- [x] displayName + sprachbezogene Demo-Konten
- [x] Changelog / Testnotizen
- [x] Kreditarten-Sortierung + Tabellenabstände
- [x] Zahlungsplan / genehmigte Laufzeit konsistent
- [x] JWT UTF-8 + customerCategory + Profilseite
- [x] BankManager-Rolle; Kunden-Löschregeln (inaktiv + keine offenen Kredite)
- [x] Gender-Allow-List; öffentliche Privatkunden-Registrierung; Mitarbeiter-Kunden-UI
- [x] Rollenbasiertes Dashboard + Statistikleiste auf allen Seiten
- [x] Inline Genehmigen / Ablehnen in der Mitarbeiterliste
- [x] Personalverwaltung-UI
- [x] Mitarbeiter-/Kundenprofilfelder + Avatar (≤2 MB) + Seq-Audit-Kategorien
- [x] BankManager: Kreditarten anlegen / aktivieren-deaktivieren
- [x] Profil-Avatar-Crop + akademischer Titel/Geschlecht + feste Positionen + Header-Avatar
- [x] Barrierefreiheits-Panel (Thema / Schriftgröße / Kontrast) + Wechselkurs-Ticker

## Offen

- [ ] API-Integrations- / E2E-Tests
- [ ] CI (GitHub Actions) Basis-Pipeline

## Später

- [ ] E-Mail-Verifizierung nach Registrierung (RabbitMQ oder Kafka)
- [ ] Elasticsearch für Mitarbeiter-GET-Endpunkte
- [ ] Admin-Welcome-Dashboard (Tailwind CSS + Bundle-Kompression / Performance)
- [ ] Kubernetes / ArgoCD Deployment
- [ ] OpenTelemetry + Grafana / Prometheus
- [ ] Produktions-Härtung (Secrets, Rate-Limit-Policies)
- [ ] OAuth (Registrierung / Login über soziale Netzwerke)
- [ ] Keycloak-Unterstützung
- [ ] Chatbot (Kunden- / Mitarbeiter-Support-Assistent)
- [ ] Datenschutz- / Cookie-Einwilligungsbanner beim ersten Laden (Speichern der Wahl)
