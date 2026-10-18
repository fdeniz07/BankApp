# Testnotizen

[Türkçe](test-notes.tr.md) · [English](test-notes.en.md)

In diesem Release geprüft (nach Umsetzung):

- [x] `dotnet build` erfolgreich
- [x] Application-Unit-Tests bestanden
- [x] Client `npm run build` erfolgreich
- [x] Compose-Rebuild (webapi + client) erfolgreich
- [x] Nach Seed stimmt Kreditart-Anzahl mit Katalog (16: 6 TR + 5 EN + 5 DE)
- [x] Mitarbeiter-Filter nach Locale (TR / EN / DE / Alle)
- [x] DE-UI Kundenformular listet nur DE + EUR Untertypen
- [x] Betrag und Laufzeit als `<select>`; freies 60 Monate / Nachkommabeträge blockiert
- [x] Create außerhalb Paket → Business Exception
- [x] Zahlungsplan-Zeilen = gewählte Laufzeit
- [x] Welcome / Topbar zeigen `displayName` (nicht E-Mail)
- [x] Demo-Konten: staff/customer + `.tr` / `.en` / `.de`
- [x] SQL-Wipe + leerer Redis zeigt volle Liste (altes 6-TR-Cache-Problem behoben)
- [x] `WebApi.IntegrationTests` Login-Smoke (3 Tests) bestanden
- [x] Integration: Genehmigen / Ablehnen / Storno / Kunden-Flows (insgesamt 9 Tests)
- [x] GitHub Actions CI-Workflow hinzugefügt (`.github/workflows/ci.yml`)
