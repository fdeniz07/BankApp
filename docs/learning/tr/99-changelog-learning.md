# Öğrenme kitaplığı günlüğü

Yeni / güncellenen bölümler burada kronolojik tutulur.

| Tarih | Değişiklik |
|-------|------------|
| 2026-10-26 | Kitaplık oluşturuldu: `00-index` + bölümler `01`–`21` + Cursor kuralı `tech-learning-library` |
| 2026-10-27 | `22-decision-letter-email.tr.md` — onay/red sonrası lab e-posta kuyruğu (`decision-letter`) |
| 2026-10-27 | `23-rate-limiting.tr.md` — auth/write/fixed politikaları; `19-vault-lab` WebApi JWT overlay |
| 2026-10-27 | `08-redis` + Compose: Redis Insight UI (`:5540`, host `redis`) |
| 2026-10-27 | `24-lab-tools.tr.md` — lab izleme araçları, kurulum / dikkat / best practice |
| 2026-10-27 | `24` + `18`: Grafana/Prometheus temel CLI ve UI adımları |
| 2026-10-27 | `09` Compose: açıklamalı komutlar, sağlık checklist, felaket/kurtarma; kural: annotated Komutlar |
| 2026-10-27 | `07` Seq, `13` RabbitMQ, `19` Vault: açıklamalı komut + checklist + kurtarma; `24` çapraz link |
| 2026-10-27 | `25-decision-letter-sms.tr.md` — onay/red SMS lab (`decision-letter-sms` → Seq) |
| 2026-10-28 | `23-rate-limiting`: ortam ayrımı (prod-benzeri vs Development) + checklist; `19-vault-lab`: rotasyon / Raft “sonra” |
| 2026-10-28 | `26-credit-application-documents.tr.md` — Pending başvuru evrak upload/list/download (App_Data) |
| 2026-10-29 | `27-branch-model.tr.md` — şube entity, staff BranchId, karar snapshot |
| 2026-10-29 | `28-branch-rbac-superadmin.tr.md` — Customer/Application BranchId, SuperAdmin, liste filtreleri, demo US şubeleri + sahte evrak |
| 2026-10-30 | `29-decision-letter-templates.tr.md` — kullanıcı karar mektubu şablon CRUD; lab şifre `Demo123!` (Personel/Manager/Customer × TR/DE/EN); katalog locale şube zorlaması |
| 2026-10-30 | `29` eklenti: soft evrak kapısı, `DecisionTemplateName` audit, doküman SignalR + eksik evrak rozeti/filtresi |
| 2026-10-30 | `21-ci-github-actions`: checkout/setup-dotnet/setup-node **v5** (Node 24; Actions Node 20 deprecation uyarısı) |
| 2026-10-30 | CI: `ubuntu-24.04` pin + nullable CS8602/CS8604 handler temizliği |
| 2026-10-30 | `29`: `SoftApproveGateEnabled` (lab kapalı) + liste preset “Bekleyen + eksik evrak” |
| 2026-11-01 | `30`: `ApproveGateMode` (None/Soft/Hard) + `ApprovalAuthority` DoA; BRD/FRD `docs/requirements/` |
| 2026-11-01 | `31`: ELK Compose (ES+Logstash+Kibana), başvuru canlı arama, Seq ile yan yana |
| 2026-11-01 | `21`: CI NuGet `actions/cache` (csproj hash); npm cache zaten vardı |
| 2026-11-01 | `21`: `actions/cache` **v5** (Node 20 deprecation uyarısı) |
| 2026-11-02 | `30`: `Branch.MaxApprovalAmount` + efektif `min(pozisyon, şube)`; config kırılımı; şube CRUD UI |
| 2026-11-02 | `30`: Şube/pozisyon DoA para birimi (`MaxApprovalCurrencyCode`, `ApprovalAuthority:CurrencyCode`) |
| 2026-11-02 | `31`: Manager/Admin ES facet paneli (terms + amount range aggregations; SQL fallback) |
| 2026-11-02 | `32`: Admin Tailwind dashboard (`/dashboard`) + Vite manualChunks + gzip_static |
| 2026-11-02 | `28`: Manager personel RBAC — başka manager’a dokunma yok; IsManager yalnız SuperAdmin (API + StaffPage) |
| 2026-11-02 | `31`: Personel ES indeksi (`bankapp-staff`) + StaffPage şube/metin/rol filtre + düzenleme modal |
| 2026-11-02 | `31`: Müşteri (`bankapp-customers`) + kredi türü (`bankapp-credit-types`) ES filtre/facet |
| 2026-11-04 | `00` + history/roadmap (TR/EN/DE): ES indeksleri + DoA PB ile senkron |
| 2026-11-06 | `33`: Keycloak lab Faz 1 — Compose (`:8180`) + `bankapp` realm import + token curl |
| 2026-11-06 | `33`: Keycloak Faz 2 — WebApi dual JwtBearer + claim enrich (`Keycloak:Enabled`) |
| 2026-11-06 | `19`: JWT dual-key rotasyon (`jwt_security_key_previous` + `vault-rotate-jwt.sh`); Raft hâlâ sonra |
| 2026-11-06 | `19`: `vault-seed` idempotent (rotate sonrası previous korunur); recreate `--no-deps` |
| 2026-11-06 | Seed: şube başına bireysel/kurumsal/personel/başvuru top-up; README TR/EN/DE senkron |
| 2026-11-07 | `33`: SPA Keycloak PKCE login (`oidc-client-ts`, `/login/callback`) |
| 2026-11-07 | `33`: Google/GitHub Identity Brokering lab notları |
| 2026-11-07 | `19`: Vault Raft HA Compose lab (3-node) |
| 2026-11-07 | README / tech-learning / `00-index` / infra README: Keycloak SPA + Vault/Raft senkron |
| 2026-11-10 | `34-a11y-portable-kit.tr.md` — taşınabilir a11y klasörü (`client/src/a11y/`) + Eye-Able benzeri lab asistan |
| 2026-11-10 | TR bölümlerde `## Where useful` → `## Nerede işe yarar?`; şablon kuralı dil uyumu |
| 2026-11-10 | EN/DE öğrenme aynası: `00-index` + `01`–`09` + changelog; Mermaid (`01`/`02`/`09` TR+EN+DE) |
| 2026-11-10 | Learning kitaplığı dil klasörlerine taşındı: `docs/learning/{tr,en,de}/` + hub `README.md` |
| 2026-11-10 | TR öğrenme bölümleri `03`–`17` ve `19`–`27`: **Bu nedir?** sonrası kompakt Mermaid akış diyagramları (backfill) |
| 2026-11-10 | Requirements dil klasörleri (tr/en/de) + EN/DE çeviri; learning EN/DE 10–18 |
| 2026-11-10 | TR `28`–`34`: **Bu nedir?** sonrası kompakt Mermaid (5–8 düğüm) |
| 2026-11-10 | EN/DE `03`–`13`, `16`, `17`: giriş bölümü sonrası Mermaid backfill (dil etiketli) |
| 2026-11-10 | Bölümler `01`–`11` (TR/EN/DE): 2+ Mermaid, benzetme, Yeni başlayan lab, Sık yapılan hatalar, Nerede işe yarar? |
| 2026-11-10 | TR/EN/DE `12`–`15`, `17`–`19`, `21`–`22`: Benzetme, Yeni başlayan lab, Sık hatalar; ≥2 Mermaid (güvenli sözdizimi); `16`/`20` hariç |
| 2026-11-10 | TR/EN/DE `23`–`31`, `33`–`34` (32 hariç): ≥2 Mermaid, Benzetme/Analogie, Yeni başlayan lab, Sık hatalar; Keycloak/a11y lab notları korundu |
| 2026-11-10 | `16`/`20`/`32` bozuk Mermaid düzeltildi + beginner zenginleştirme; `tech-learning-library` kuralı: ≥2 görsel, lab, hatalar, Mermaid safety |
| 2026-11-10 | `26` evrak: Pending silme + önizleme + UI hizalama notları (TR/EN/DE) |
| 2026-11-10 | `28`: Manager personel UI yalnız kendi şube; navbar şube chip (TR/EN/DE) |
| 2026-11-10 | Screenshot galerileri + README: yeni yakalar (SuperAdmin/Keycloak/Kibana/Redis); kırık yollar düzeltildi; learning `06`/`08`/`18`/`28`/`31`–`33` |
| 2026-11-10 | Learning gömülü ekranlar: `07` Seq, `18` Prom/Grafana, `22`/`29` mektup kalıbı, `31` Kibana, `34` a11y (TR/EN/DE) |
