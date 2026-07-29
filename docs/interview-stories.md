# TicketReserve — Mülakat & Öğrenme Notları

Bu proje boyunca öğrenilen, mülakatta savunulabilir kavramlar ve karşılaşılan gerçek hatalar. Konu konu düzenlenmiştir.

---

## 1. Dual-Write Problemi & Transactional Outbox

**Problem:** İş verisini (DB) yazıp ayrıca mesaj kuyruğuna publish etmek **atomik değildir**. Biri olur öbürü olmazsa tutarsızlık. Örn: satış commit oldu ama tam o an process çöktü → bildirim asla gitmez, hata da yok.

**Çözüm — Outbox:** Bildirimi ayrı publish olarak değil, iş verisiyle **AYNI transaction** içinde bir `OutboxMessages` satırı olarak yaz. İkisi ya birlikte commit ya birlikte rollback → kayıp imkânsız (tek DB'ye yazdığın için atomik). Ayrı bir dispatcher satırı okuyup broker'a iletir.

**Neden 2PC değil:** İki kaynağı tek transaction'a sokmak (distributed transaction / two-phase commit) availability düşürür, koordinatör tek hata noktasıdır, çoğu modern DB/broker düzgün desteklemez. Outbox tam da 2PC'den kaçmak için var.

**Outbox kuralı:** Outbox, koruduğu veriyle **aynı DB'de** olmalı (tek local tx ikisini tek kadere bağlar). Servisler ayrılınca (mikroservis) her servis kendi DB+outbox'ını taşır; transaction hep bir servisin içinde local kalır.

**Dual-write neden patlamayla çözülmez:** İki ayrı transaction'da ilk yazım commit olduysa, sonraki adımın exception'ı onu geri **alamaz** (500 döner ama kullanıcı DB'de var = yalan hata). Bazen crash olur, exception bile yok = sessiz kayıp. Tek transaction'da patlama commit'ten ÖNCE olur → her şey rollback → tutarlı. Mesele "patlıyor mu" değil "**patlama ilk yazımı geri alabilecek konumda mı**".

> **Mülakat:** "Dual-write'ı transactional outbox ile çözdüm — bildirimi iş verisiyle aynı transaction'da bir outbox satırına yazıp ayrı bir dispatcher'ın broker'a iletmesini sağladım. 2PC'ye girmeden atomiklik kazandım."

---

## 2. Teslim Garantileri: At-Least-Once + Inbox = Exactly-Once Processing

**At-least-once nereden gelir:** Dispatcher "önce publish, sonra damgala" yapar → arada çökerse satır işaretlenmemiş kalır → tekrar publish edilir. Ayrıca consumer `autoAck:false` + hata'da `Nack(requeue:true)`. Sonuç: mesaj **asla kaybolmaz ama bazen tekrarlanır**.

**Inbox (idempotency):** Consumer, işe başlamadan `ProcessedMessages` tablosunda "bu MessageId'yi işledim mi?" diye bakar. İşlediyse tekrarlamaz, sadece ack atar. Bileşik anahtar `(MessageId, Consumer)` → aynı mesajı email ve sms **ayrı ayrı** idempotent işler (tek MessageId, iki satır).

**Sıra kutsal:** İş + Inbox yazımı commit olmadan **ack yok**. Save ile ack arası çökersek mesaj tekrar gelir, Inbox yakalar.

**Neden ikisi birlikte:** Sadece at-least-once → çift mail. Sadece idempotency + at-most-once → kayıp. İkisi → **exactly-once processing** (teslim değil, *işleme* seviyesinde). Gerçek "exactly-once delivery" imkânsıza yakın.

> **Mülakat:** "At-least-once teslim + Inbox tablosuyla idempotency = exactly-once processing. Exactly-once delivery pratikte imkânsız; consumer'ı idempotent yaparak işleme seviyesinde çözersin."

---

## 3. RabbitMQ Temelleri

- **Exchange tipleri:** **Topic** (routing key deseniyle seçici, `seat.*`), **Direct** (birebir), **Fanout** (routing key'e bakmaz, bağlı tüm kuyruklara kopyalar), **Headers**.
- **Akış:** Producer → **exchange** → (binding + tipe göre) → **queue(lar)** → consumer.
- **Fan-out:** Aynı routing key'e iki kuyruk bind → mesajın kopyası ikisine de gider (bizde: satış olayı hem email hem sms kuyruğuna).
- **Ack:** `autoAck:false` + iş bitince manuel `BasicAck`. Hata → `Nack(requeue:true)`.
- **`IConnection` thread-safe (paylaşılır, singleton), `IChannel` thread-safe DEĞİL** (her consumer kendi channel'ını açar).

---

## 4. Mesaj Tasarımı

- **Integration vs Domain event:** Integration event process/broker sınırını geçer (`<GeçmişZamanOlay>IntegrationEvent`, eShopOnContainers); domain event servis içinde kalır.
- **Event alanını sözleşmenin anlamına göre adlandır:** `UserName` değil `Email` — producer iç modeli tüketiciye sızmamalı.
- **Type header deseni:** Mesajın tipi **zarfta** (`BasicProperties.Type`); consumer body'yi açmadan tipi okuyup doğru DTO'ya deserialize eder. Sektör: MassTransit `messageType`, CloudEvents `type`, Azure Service Bus `Subject`.
- **`nameof` vs string kontrat:** Tek deploy'da (monolith) `nameof` derleyici garantisi verir; process ayrılınca string kontrat (`"user.registered.v1"`) gerekir. Versiyonlamayı erken yapmak = garantiyi zayıflatmak + spekülatif genellik.
- **Event = OLAY (fact), kanal payload'u değil:** Tek event'i hem email hem sms dinler. Email daha çok veri isterse (adres) → **event değişmez**, email servisi detayı DB'den kendi çeker (thin event). Kanal başına ayrı event YAPMA (producer kanalları bilmemeli).
- **Fat vs Thin event:** Fat (Event-Carried State Transfer) = her şeyi taşı (consumer bağımsız, event şişer). Thin = anahtarları taşı, consumer çeker (event küçük, consumer DB'ye bağımlı). Mikroservise ayrılınca fat'e ya da read model'e geçilir.

---

## 5. Poison Message & Dead-Letter Queue (DLQ)

- **Hata türü ayrımı:** Geçici hata (DB down → `requeue:true`) vs kalıcı hata (bozuk JSON → `requeue:false`, döngüyü kır). Catch sırası: **spesifik üstte** (`when (ex is JsonException or FormatException)`).
- **Dead-letter exchange:** Kuyruğa `x-dead-letter-exchange` argümanı → `Nack(requeue:false)` mesaj silinmez, **karantina kuyruğuna** yönlenir. DLX **Fanout** seçilir çünkü ölü mesaj orijinal routing key'ini taşır; fanout key'e bakmadığı için hepsi tek DLQ'ya düşer.
- **`x-death` header:** Ölen mesajın sebebini/sayısını/geldiği kuyruğu taşır.
- **Turnike (retry counter) deseni:** AMQP mesajda deneme sayacı tutmaz (sadece `redelivered`) → ana kuyruk → TTL'li retry kuyruğu → geri, her turda `x-death.count`+1, eşik aşılınca gerçek DLQ. MassTransit'te tek satır: `UseMessageRetry`.

> **GERÇEK BUG (test buldu, kod incelemesi kaçırdı):** DLQ declare'inde `exclusive` atlandı → RabbitMQ.Client varsayılanı `exclusive:true`. Sonuç: kuyruk declare eden bağlantıya kilitlendi (`RESOURCE_LOCKED`) ve API restart'ında **DLQ + mesajları silindi** (canlı kanıtlandı — connection kapanınca exclusive kuyruk auto-delete). **Ders:** `QueueDeclareAsync` varsayılanları geçici kuyruk için (`durable=false, exclusive=true, autoDelete=true`); kalıcı altyapı kuyruğunda üç bayrağı da açıkça yaz.
> - **durable** = broker restart'ında kalır (+mesajın da `Persistent` olması şart).
> - **exclusive** = bağlantıya özel + bağlantı kapanınca silinir.
> - **autoDelete** = son consumer ayrılınca silinir.

---

## 6. MassTransit (elle yazımı framework'e devretme)

**Ne veriyor:** Elle yazdığımız her şey framework'te — connection/channel yönetimi, topoloji (`ConfigureEndpoints` kuyruk/exchange/binding'i otomatik kurar), ack/nack, retry, EF Core Outbox+Inbox, `_error` kuyruğu (DLQ), correlation, OpenTelemetry.

**Yapı (idiomatik):**
- **Consumer = ince adaptör** (`IConsumer<T>`): mesajı karşılar, işi bir **servise** devreder. İçinde deserialize/ack/inbox/log YOK.
- **Servis (interface arkasında):** `IEmailNotificationService` → `EmailNotificationService`. **DIP:** consumer somuta değil soyutlamaya bağımlı → test'te mock, SMTP'ye geçişte consumer değişmez. Kanal başına tek interface (ASP.NET Identity `IEmailSender` deseni). Konum: `Infrastructure/Notifications/` (SeatLock deseniyle tutarlı — yetenek servisleri Features değil Infrastructure).
- **Bir sınıf çok `IConsumer<T>`:** `EmailConsumer : IConsumer<SeatsPurchased>, IConsumer<UserRegistered>` → kanal gruplaması korunur, framework tipe göre doğru `Consume`'u çağırır (elle `switch`'in yerine).
- **Producer:** `IPublishEndpoint.Publish(...)` (SaveChanges'tan ÖNCE). `UseBusOutbox` açıkken Publish broker'a gitmez; DbContext SaveChanges interceptor'ı mesajı aynı transaction'da outbox tablosuna yazar, teslim servisi sonra publish eder. `RegisterHandler`'da explicit transaction hâlâ gerekli (UserManager kendi SaveChanges'ı).

**Kurulum:** `AddMassTransit(SetKebabCaseEndpointNameFormatter + AddConsumer + AddEntityFrameworkOutbox<Ctx>(UsePostgres+UseBusOutbox) + UsingRabbitMq(Host + ConfigureEndpoints))` + DbContext'e `AddInboxStateEntity/AddOutboxStateEntity/AddOutboxMessageEntity` + migration.

**Lisans (önemli):** MassTransit **v9 ticari/ücretli**, **v8 ücretsiz Apache-2.0 LTS**. Portföy/prod için v8'e sabitle (8.5.10).

**Migration deseni:** **Coexistence-then-cutover** — önce MassTransit'i eskinin YANINA kurup test ettim (yeni hat çalışıyor + eskiye artık yazılmıyor kanıtı), sonra eskiyi tek seferde söktüm (kod + drop-table migration). Big-bang değil, güvenli.

**Yolda çıkan hata:** `using MassTransit` → MassTransit'in `Event` tipi bizim `Data.Entities.Event`'le çakıştı (CS0104). Fix: `using Event = TicketReserve.Api.Data.Entities.Event;` alias.

**Alternatifler:** NServiceBus (Outbox built-in), Wolverine (base class yok, konvansiyon).

> **Mülakat:** "Outbox/inbox/consumer/DLQ'yu önce elle yazıp mekaniğini öğrendim, sonra MassTransit'e migrate ettim — altında ne yaptığını bildiğim için. Prod'da framework kullanırım ama debug edebilirim çünkü aynısını elle yazdım."

---

## 7. Eventual Consistency

İş verisi tek local transaction ile **strong**; yan etki (bildirim) outbox+broker ile **eventual**. Sınır transaction'ın bittiği yerden geçer (içeri strong, broker'ı geçen dışarı eventual). Çözdüğü: 2PC'den kaçış + decoupling (email çökse satış bloklanmaz) + availability. Bedeli: "satış oldu ama mail gitmedi" penceresi normaldir.

---

## 8. Observability — OpenTelemetry Distributed Tracing

**Problem:** Async akışta düz log'lar bir isteği takip edemez; "mail neden geç gitti" (HTTP mu/kuyruk mu/consumer mı/DB mi) tahmin edilir.

**Kavramlar:**
- **Trace** = isteğin tüm yolculuğu (tek `TraceId`).
- **Span** = tek işlem (ad + süre + parent). İç içe nesting = **şelale**.
- **Context propagation:** process içi `Activity.Current`, HTTP'de **`traceparent`** header (W3C Trace Context), MassTransit mesajlarında traceparent'ı header'a **enjekte/çıkararak async sınırı bağlar**.

**OpenTelemetry = sektör standardı.** Zihinsel model: **instrumentation** (otomatik span üreticiler: ASP.NET Core, Npgsql, MassTransit) + kullandığın lib'lerin **ActivitySource'unu kaydet** (`AddSource("MassTransit")`) + **exporter** (OTLP) → **backend** (Jaeger, şelale UI).

**Kritik anlayış:** traceparent inject/extract = **MassTransit otomatik yapıyor, sıfır kod**. `AddSource("MassTransit")` propagation'ı değil, onun **görünürlüğünü** açar. (Metafor: MassTransit hep damgalıyor; biz sadece kamera taktık.)

**Log-trace bağı:** Serilog `Enrich.WithSpan()` (Serilog.Enrichers.Span) → log'a `TraceId` → log'dan trace'e atlarsın.

**Uçtan uca test kanıtı:** Kayıt isteği → Jaeger'da tek trace şelalesi: `POST /register` (kök, 508ms) → postgresql×12 → outbox send → outbox process → publish → email receive → email process. Async span'ler (outbox process, email) HTTP cevabından SONRA (553-621ms) çalıştı = **eventual consistency zaman çizgisinde görünür**.

> **Mülakat:** "Async akışta isteği uçtan uca takip için OTel distributed tracing kurdum. Her istek TraceId alır; ASP.NET Core/MassTransit/Npgsql instrumentation otomatik span üretir. MassTransit W3C trace context'i (traceparent) mesaj header'ına enjekte edip consumer'da çıkararak aynı trace'i async sınırın ötesinde sürdürür → HTTP→publish→kuyruk→consume→DB'yi Jaeger'da tek şelale görürüm." Neden elle correlation değil: "OTel standart+otomatik+zamanlama verir; 'mail neden geç' sorusunu kuyrukta 4.8sn beklediğini GÖREREK cevaplarım."

---

## 9. Entegrasyon Testleri (Testcontainers + WebApplicationFactory)

**Neden entegrasyon (unit değil):** Projenin değeri altyapı etkileşiminde (seat lock concurrency, xmin, outbox); mock'la yakalanmaz, gerçek DB/Redis ister. **Testcontainers** = gerçek Postgres/Redis/RabbitMQ'yu geçici konteynerde ayağa kaldır, bitince sil = yüksek sadakat, sektör standardı. **WebApplicationFactory** = API'yi in-memory host olarak kaldırır, gerçek HTTP isteği atarsın.

**Yıldız test — concurrency:** Aynı koltuğa iki kullanıcı `Task.WhenAll` ile paralel lock+purchase → **tam biri başarılı** (`Count(IsSuccessStatusCode)==1`). Kanıt: A=200 (kilitledi+aldı), B=409 (Seat.AlreadyLocked → LockNotOwned). Redis lock'ın gerçek paralellikte mutual exclusion sağladığını ispatlar.

**Collection fixture (pahalı fixture'ı paylaş):** `IClassFixture` (sınıf başına) → iki sınıf paralel koşup her biri kendi konteynerini+host'unu kurunca WebApplicationFactory'nin statik host-yakalama kancası yarışıyor → "entry point exited without building an IHost". Fix: `ICollectionFixture` + `[CollectionDefinition]` + `[Collection]` → tek fixture tüm sınıflarca paylaşılır, sıralı koşar. İzolasyon: paylaşılan DB'de her test benzersiz veri (Guid email/event).

**Lisans:** FluentAssertions v8 ticari → v7'ye sabitle (Shouldly MIT alternatif).

> **Mülakat:** "Entegrasyon testlerini Testcontainers + WebApplicationFactory ile yazdım; yıldız test aynı koltuğa iki paralel purchase atıp tam birinin başarılı olduğunu doğruluyor — Redis lock'ı gerçek altyapıyla ispatlıyor."

---

## 10. Karşılaşılan Gerçek Bug'lar & Tuzaklar (en iyi mülakat malzemesi)

- **`SaveChanges ≠ Commit`:** Explicit transaction açıkken `SaveChanges` commit ETMEZ; commit'i sen verirsin. `RegisterHandler`'da gerekti (UserManager kendi SaveChanges'ını yapıyor).
- **`BackgroundService` + Scoped DbContext:** Singleton servis Scoped DbContext'i doğrudan enjekte edemez → `IServiceScopeFactory.CreateScope()`.
- **`ILogger` vs `ILogger<T>`:** Non-generic `ILogger` DI'da yok → runtime crash; hep `ILogger<T>`.
- **Config'i CHANGE ederek doğrula:** `Configure<SeatLockSettings>` hiç kaydedilmemişti; `IOptions<T>` sessizce `new T()` default kullanıp bug'ı maskeledi. Değeri değiştirip test edince yakalandı.
- **DLQ `exclusive:true` bug'ı** (bkz. bölüm 5) — testin bulduğu, portföyün en iyi hikayesi.
- **WebApplicationFactory config-timing tuzağı:** JWT doğrulama anahtarı app açılışında (build'den ÖNCE) okunuyor, test config override'ı o an yok → token test anahtarıyla imzalanıp farklı anahtarla doğrulanınca **401**. Fix: `ConfigureTestServices` + `PostConfigure<JwtBearerOptions>`. Genel ders: startup'ta erken-okunan config test override'ını görmez (IOptions geç-okuma görür).
- **Migration-seeder sırası (test):** Seeder host kurulurken tablolara sorar → şemayı `factory.Services`'e dokunmadan, elle `new DbContext` ile ÖNCE migrate et (Services'e dokunmak = host kur = seeder çalış).
- **Tanı yöntemi:** Response `StatusCode` + body'yi assert mesajına koyup gerçek hatayı gör (401 mi 403 mü, gövde ne) — tahmin etme, gördür.
- **MassTransit v9 / FluentAssertions v8 lisans:** Kütüphane eklerken lisansı kontrol et; ücretsiz LTS sürümüne sabitle.
