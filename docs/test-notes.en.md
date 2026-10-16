# Test notes

[Türkçe](test-notes.tr.md) · [Deutsch](test-notes.de.md)

Verified in this release (after implementation):

- [x] `dotnet build` succeeded
- [x] Application unit tests passed
- [x] Client `npm run build` succeeded
- [x] Compose rebuild (webapi + client) succeeded
- [x] After seed, credit type count matches catalog (16: 6 TR + 5 EN + 5 DE)
- [x] Staff credit-types locale filter (TR / EN / DE / All)
- [x] DE UI customer form lists only DE + EUR sub-types
- [x] Amount and term are `<select>`; free-form 60 months / fractional amounts blocked
- [x] Out-of-package create returns business exception
- [x] Payment plan row count equals selected term
- [x] Welcome / top bar show `displayName` (not email)
- [x] Demo accounts: staff/customer + `.tr` / `.en` / `.de`
- [x] SQL wipe + empty Redis shows full list (old 6-TR cache issue fixed)
