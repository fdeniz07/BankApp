# Änderungsprotokoll

[Türkçe](changelog.tr.md) · [English](changelog.en.md)

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
