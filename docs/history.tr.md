# Proje geçmişi

[English](history.en.md) · [Deutsch](history.de.md)

Kronolojik özet (eğitim / kurumsal BankApp çalışması).

## Faz 1 — Temel API

- Clean Architecture katmanları (Core, Domain, Application, Persistence, WebApi)
- Müşteri CRUD (bireysel / kurumsal, TPT)
- Kredi tipi, alt tip, başvuru; onay / red; soft delete
- FluentValidation, iş kuralları, AutoMapper, MediatR pipeline
- SmartEnum (durum, kategori, risk)
- Development Bogus seed

## Faz 2 — Güvenlik ve gözlemlenebilirlik

- ASP.NET Identity + JWT
- Customer / BankStaff rolleri (`ISecuredRequest`)
- ApplicationUser ↔ Customer eşlemesi
- Serilog + Seq
- Redis cache (kredi tipi / alt tip listeleri)

## Faz 3 — Altyapı

- Docker Compose: Seq, Redis, SQL Server (external volume), WebApi
- `infra/.env.example`, README
- `bin` / `obj` gitignore ve temizleme
- Application unit testleri

## Faz 4 — React istemci

- Vite + React + TypeScript istemci
- TR / EN / DE i18n (`react-i18next`)
- Giriş, başvuru listesi / detay, personel onay-red
- Client Docker (nginx) Compose’a eklendi
- Müşteri adı / kategori, para birimi kolonları
- EN / DE seed müşteri ve USD / EUR ürünler; TR ürünlerde TRY
- Müşteri paneli, yeni başvuru, iptal (Cancel)
- Liste sıralama ve durum filtresi

## Faz 5 — Veri kalitesi ve UX (güncel)

- Alt tür adları ürüne göre TR / EN / DE eklerle uyumlu
- Yuvarlak tutarlar (örn. 50.000 ₺, 250.000 €), vade adımları 3–6–9–12–15–18–24–32
- Mevcut kayıtlar için seed sync (SQL wipe gerekmez)
- Tablo kayması giderimi; onay / red ve Detay / İptal buton boyutu eşitlendi
- `docs/` altında yol haritası ve geçmiş (TR / EN / DE)
