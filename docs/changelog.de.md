# Änderungsprotokoll

[Türkçe](changelog.tr.md) · [English](changelog.en.md)

## 2026-10-21 — Domain Events + Transactional Outbox

- `AggregateRoot` + Kreditantrags-Events (Created / Approved / Rejected / Cancelled).
- Tabelle `OutboxMessages`; Schreiben in derselben Transaktion wie `BankingDbContext.SaveChanges`.
- `OutboxDispatcherHostedService` → RabbitMQ-Queue `domain-events`; Consumer loggt nach Seq/Konsole.
- Unit- + Integrationstests (Outbox-Zeilen).

## 2026-10-21 — Phase-6-Architektur-Roadmap + Diskussionsnotizen

- Roadmap: **Phase 6 — Architekturtiefe** (Outbox, Idempotency, Saga, Modular Monolith, SignalR).
- Diskussionsarchiv: [architecture-notes.de.md](architecture-notes.de.md) (TR/EN-Pendants).
- History- und README-Dokumentlinks aktualisiert.

## 2026-10-19 — Docker-Compose-Healthchecks

- Redis / Seq / WebApi (`/health`) / Client-Healthchecks; `depends_on: service_healthy`-Kette.
- WebApi-Image mit `curl`; nur Liveness (kein DB/Redis-Probe).

## 2026-10-19 — Kunden-Create/Detail-Felder + Katalog-Vorschläge

- Privat/Firma-Create-Formulare an Detailfelder angeglichen (Passwort nur bei Create).
- Firma Typ/Sektor/Branche/Unterbranche: Datalist (wählen oder neu tippen); `GET …/catalog`.

## 2026-10-19 — Automatische Kundennummer / nur lesen

- Mitarbeiter-Anlage: System vergibt Nummer (`IND|CORP-{TR|EN|DE}-…`); kein Formularfeld.
- Detail/Update: Nummer sichtbar, nicht änderbar (API ignoriert).

## 2026-10-19 — Kundennummer-Locale + Mitarbeiter-Detailfelder

- Registrierungs-Kundennummer: `IND-{TR|EN|DE}-{yyyyMMdd}-{hex}` (UI-Sprache).
- Mitarbeiter-Kundendetail: Privat Geburtsdatum/Eltern; Firma E-Mail/Adresse/Typ/Sektor/Branche/Gründung.

## 2026-10-19 — Angemeldet bleiben + Kundenprofil-Felder

- Login: „Angemeldet bleiben“ → längeres JWT (7 Tage) + `localStorage`; unchecked → kurzes JWT + `sessionStorage`; E-Mail merken.
- Kundenprofil: Privat (Geburtsdatum / Eltern + Nur-Lesen Nr./Ausweis), Firma (Steuer / Handelsregister).

## 2026-10-19 — E-Mail-Bestätigung (RabbitMQ)

- Privatkunden-Registrierung liefert kein JWT; `EmailConfirmed` + Token (24 Std.); Kunden-Login erfordert Bestätigung.
- `POST /api/Auth/confirm-email`; Client `/confirm-email?token=…`.
- RabbitMQ-Queue (Compose); bei Broker-Ausfall erscheint der Link in Seq/Console-Logs.
- Bestehende / Seed-Benutzer sind bestätigt; neue Registrierungen warten auf Bestätigung.

## 2026-10-19 — Datenschutz- / Cookie-Einwilligung

- Banner beim ersten Laden (Akzeptieren / Ablehnen); Wahl in `localStorage` (`bankapp-consent`).
- Texte TR / EN / DE; Layout freihält die Barrierefreiheits-FAB.

## 2026-10-18 — CI + Integrationstests

- GitHub Actions CI: `dotnet build/test` (Unit + Integration) und Client-npm-Build.
- `WebApi.IntegrationTests`: InMemory-DB + Memory-Cache-Factory; Login-Smoke.
- Flows: Create→Genehmigen, Create→Ablehnen, Staff/Kunden-Storno; Kunde darf nicht genehmigen (401); Create ohne Token → 401.

## 2026-10-16 — Barrierefreiheit, Wechselkurs-Ticker, Details-Button

- Barrierefreiheits-Panel unten links: hell/dunkel, Schriftgröße, hoher Kontrast (`localStorage`).
- Oben scrollende Wechselkurse (USD/TRY, EUR/TRY usw.; open.er-api mit Fallback).
- Listen-**Details**-Button schieferblau (`btn info`); Aktionsabstand vergrößert.

## 2026-10-16 — Profil-Crop, Titel/Geschlecht, feste Positionen, Header-Avatar

- Avatar: im Rahmen ziehen / zoomen / zentrieren, dann hochladen.
- Akademischer Titel (`Dr`/`Prof`) vom Geschlecht getrennt.
- Mitarbeiterposition aus fester SmartEnum-Liste (lokalisierte Labels).
- Runder Avatar in der Topbar; Name/Bild → `/profile`.

## 2026-10-16 — Manager-Kreditartenverwaltung

- Create / Update / Delete nur für `BankManager`.
- `PUT /api/CreditTypes/{id}/active` zum Aktivieren/Deaktivieren.
- Neue Anträge bei inaktiver übergeordneter Kreditart gesperrt.
- UI: Manager-Anlageformular + Zeilen-Aktivierung.

## 2026-10-16 — Profil, Avatar, Seq-Audit

- `ApplicationUser`-Profilfelder: FirstName, LastName, PersonTitle (SmartEnum), DateOfBirth, Position, ProfileImagePath.
- Mitarbeiter können eigenes Profil aktualisieren; Manager bearbeiten alle (E-Mail/Passwort/Status/Rolle).
- Avatar-Upload: JPEG/PNG/WebP, max. 2 MB, Magic-Byte-Prüfung; `wwwroot/uploads/profiles`.
- Strukturiertes Seq-Audit: Auth / Credit / Staff / Profile (`EventCategory`, `EventAction`).
- E-Mail ohne türkische Zeichen; JWT-`nameidentifier`-Claim korrigiert (SOAP-URI).

## 2026-10-16 — Dashboard-Statistiken (alle Rollen)

- `GET /api/Dashboard/summary`: Mitarbeiter sehen aktive/inaktive Kunden + Antragsaufteilung; Kunden sehen eigene Antragszahlen.
- Statistikleiste auf jeder Seite (Kundenzahl für Personal; Anträge/Ausstehend für alle).
- Rollenbasiertes Dashboard auf der Antragsseite; Zahlen aus der API (nicht aktuelle Seitenzeilen).

## 2026-10-16 — Kundenverwaltung, BankManager, Privatkunden-Registrierung

- Claim `BankManager` + Demo `manager@bank.local` / `Manager123!` (BankStaff + BankManager).
- Soft-Delete nur für BankManager; Kunde muss inaktiv sein und keine Pending/Approved-Anträge haben.
- `Gender`-Allow-List (`Female` / `Male` / `PreferNotToSay`) bei Create / Update / Register.
- Öffentliches `POST /api/Auth/register-individual` + Seite `/register`.
- Mitarbeiter-`/customers`-Liste (aktiv / inaktiv / gelöscht, Privat / Firma), Erstellformular und Detail (IsActive, Löschen).

## 2026-10-16 — UX-Bugfix (Abstand, Sprache, Kategorie, Toast)

- Stärkerer Abstand Name/Code bei Kreditarten.
- Login speichert `displayName` / `customerCategory` per Local-Override (türkische Zeichen).
- Seed-Monatseinkommen an beantragten Betrag gekoppelt.
- Kundenformular zeigt nur passende Privat-/Firmenprodukte; API-Filter `category`.
- Leeres `CustomerId` bei Create-Validation entfernt; `react-toastify` + lokalisierte Fehler.

## 2026-10-16 — UX-Korrekturen, Profil, JWT UTF-8

- Kreditarten-Liste: Spaltensortierung (`SortBy` / `SortDesc`) und Abstand Name/Code.
- Antragsdetail zeigt genehmigte Laufzeit; Zahlungsplan nennt wirksamen Betrag/Laufzeit (Pending-Vorschau / genehmigte Werte).
- Seed: genehmigte Laufzeit überschreitet die beantragte nicht; oft gleich.
- JWT `displayName` UTF-8-sicher; Claim `customerCategory`; aussagekräftige Mitarbeiter-Namen.
- Seite `/profile` für Kundenname/Kontakt; Header zeigt Kundentyp.
- Roadmap Später: E-Mail-Verifizierung (RabbitMQ/Kafka), Elasticsearch, Tailwind-Admin-Dashboard.

## 2026-10-16 — Locale-Katalog, Paketregeln, Zahlungsplan

- `CreditType.Locale` hinzugefügt; Kataloge TR→TRY, EN→USD, DE→EUR neu geseedet.
- Kunden-UI zeigt nur Produkte der aktiven UI-Sprache; Betrag/Laufzeit aus Paketoptionen.
- Mitarbeiterseite Kreditarten mit Filter TR/EN/DE.
- Server-seitige Paketvalidierung (Betragsraster + erlaubte Laufzeiten 3…60).
- Amortisations-Zahlungsplan API + UI-Vorschau.
- Login `displayName` (Name / Firma); sprachbezogene Demo-Konten.
- SQL-Volume + Redis zurückgesetzt (sauberer Seed).
- Docs: Changelog, Testnotizen, Roadmap/History aktualisiert.
