# 31 — ELK (Elasticsearch + Logstash + Kibana)

## Bu nedir?

**ELK**, üç parçalı bir gözlem / arama yığınıdır:

| Parça | Rol |
|--------|-----|
| **Elasticsearch** | JSON belgeleri index’ler; full-text + filtre sorguları |
| **Logstash** | Veriyi alır (HTTP/TCP/Beats), dönüştürür, ES’e yazar |
| **Kibana** | Index keşfi, Discover, dashboard UI |

BankApp lab’de ELK **iki iş** için kullanılır:

1. **Kredi başvurusu araması** (ürün read-model → ES; React canlı arama kutusu)  
2. **Uygulama logları** (Serilog → Logstash HTTP → `bankapp-logs-*`; Kibana’da aranabilir)

**Seq silinmedi.** Seq .NET yapısal log için hızlı lab UX’tir; ELK ise arama motoru + Kibana’dır. İkisi yan yana çalışır.

## Bu projede ne işe yarar?

- Personel listesinde yazdıkça arama (`searchText`, ~300 ms debounce)
- Staff: şube scoped; Manager/SuperAdmin: daha geniş filtre
- ES kapalıysa SQL `Contains` fallback
- Compose: `elasticsearch` :9200, `logstash` :8081/:50000, `kibana` :5601

## Ne zaman eklendi?

Faz 6 — Soft/Hard kapı + DoA sonrasında (roadmap: personel GET araması).

## Temel kavramlar

| Kavram | Anlam |
|--------|--------|
| Index | Tablo benzeri belge koleksiyonu (`bankapp-credit-applications`, `bankapp-logs-YYYY.MM.dd`) |
| Document | Tek JSON kayıt (başvuru veya log olayı) |
| Mapping | Alan tipleri (text / keyword / date…) — lab’de çoğu dinamik |
| Query | `multi_match`, `term`, `bool` filtre |
| Pipeline | Logstash input → filter → output |
| Discover | Kibana’da index pattern ile log/arama gezintisi |

### Seq mi, Elasticsearch mi? (log için)

| | **Seq** | **Elasticsearch + Kibana** |
|--|---------|----------------------------|
| Amaç | .NET yapısal log, hızlı debug | Genel arama, dashboard, çok kaynak |
| Kurulum | Hafif | Daha ağır (RAM) |
| Günlük DX | Serilog sink, güzel UI | Güçlü ama ops maliyeti |
| BankApp lab | **Birincil log UX** | Kibana’da log **da** aranabilir (opsiyonel derinlik) |

**Sonuç:** Seq’i ES ile **değiştirme**; log aramasını Kibana ile **genişlet**. Üretimde bazı ekipler yalnızca ELK tutar — lab’de ikisini bilmek daha öğretici.

## Komutlar

### Yığını ayağa kaldır

```powershell
cd infra
copy .env.example .env   # yoksa
docker compose --env-file .env up -d elasticsearch logstash kibana
```

Sonra: `docker compose --env-file .env ps` → elasticsearch/kibana healthy olmalı.  
RAM yetmezse `.env` içinde `ELASTIC_JAVA_OPTS=-Xms256m -Xmx256m` dene.

### Elasticsearch sağlık

```powershell
curl.exe -s http://localhost:9200
curl.exe -s http://localhost:9200/_cluster/health?pretty
```

Beklenen: `status` green/yellow (tek düğümde yellow normal olabilir).

### Index listesi

```powershell
curl.exe -s http://localhost:9200/_cat/indices?v
```

WebApi ayağa kalkınca `bankapp-credit-applications` ve (log geldikçe) `bankapp-logs-*` görünür.

### Logstash’e test (TCP json_lines)

```powershell
# Tek satır JSON (Logstash :50000)
echo '{"message":"lab-tcp-test","level":"Information"}' | curl.exe -s telnet://127.0.0.1:50000
```

Windows’ta TCP için alternatif: WebApi’yi Logstash açıkken çalıştırıp Seq + Kibana Discover’ı kontrol et.

### Kibana

Tarayıcı: http://localhost:5601  

1. Stack Management → Data Views → `bankapp-logs-*` (timestamp alanı)  
2. Discover → logları filtrele  
3. İkinci data view: `bankapp-credit-applications` (ürün araması index’i)

### WebApi + arama

```powershell
# Compose full stack veya lokal API + ES
curl.exe -s "http://localhost:8080/api/CreditApplications?PageIndex=0&PageSize=10&searchText=APP" `
  -H "Authorization: Bearer <token>"
```

## Sınıflar, metotlar, paketler

- `ElasticsearchOptions`, `LogstashOptions`
- `ICreditApplicationSearchService` / `ElasticCreditApplicationSearchService` / `Null…`
- `GetListCreditApplicationRequest.SearchText`
- NuGet: `Elastic.Clients.Elasticsearch`, `Serilog.Sinks.Http`, `Serilog.Sinks.Elasticsearch` (Logstash kapalıyken doğrudan ES)
- Client: `ApplicationsPage` debounce + `getApplications({ searchText })`

## Repoda nerede?

- `infra/docker-compose.yml` — elasticsearch, logstash, kibana
- `infra/logstash/pipeline/logstash.conf`
- `infra/logstash/config/logstash.yml`
- `src/WebApi/Modules/Credit/Search/`
- `src/Application/Services/Search/`
- `src/Core/Logging/SerilogHostBuilderExtensions.cs`
- `client/src/pages/ApplicationsPage.tsx`

## Mini akış

1. Compose ES+Logstash+Kibana  
2. WebApi start → `ReindexAllAsync` → başvurular index  
3. Personel listede yazar → 300 ms sonra `searchText` → ES `multi_match` (yoksa SQL)  
4. Serilog → Logstash HTTP → `bankapp-logs-*` → Kibana Discover  

## Where useful

- [ ] `curl localhost:9200` cevap veriyor  
- [ ] Kibana açılıyor; log data view oluşturulabiliyor  
- [ ] Listede arama kutusu yazdıkça sonuç yenileniyor  
- [ ] ES durunca liste SQL fallback ile çalışıyor  
- [ ] Staff şube dışı sonuç görmüyor  

### Felaket / kurtarma (az yıkıcı → çok)

1. `docker compose restart elasticsearch`  
2. Index sil / yeniden oluştur: WebApi restart (reindex)  
3. Volume sil (**veri gider**):  
   `docker compose down` sonra `docker volume rm bankapp-elasticsearch-data` — `-v` tüm named volume’ları da etkiler; SQL external volume’a dikkat  

Lab notu: Docker Desktop’ta ES bazen yavaş ayağa kalkar (`start_period` 60s+).
