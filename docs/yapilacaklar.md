# Yapılacaklar / Derinleşme Backlog'u

## Sıradaki iş: Poison Message Merdiveni (RabbitMQ consumer)

Bağlam: `PurchaseNotificationConsumer` catch'inde `BasicNack(requeue: true)` her hatayı geri koyuyor —
bozuk mesaj (deserialize edilemeyen JSON) sonsuz döngüye girer. Çözüm üç basamak, sırayla:

### Basamak 1 — Hata türü ayrımı (~10 dk)
- [ ] Mevcut `catch (Exception)` üstüne `catch (Exception ex) when (ex is JsonException or FormatException)` ekle
- [ ] Kalıcı hata dalında `requeue: false` + `LogError` (catch sırası: spesifik üstte)
- Kavram: geçici hata (DB down → tekrar dene) vs kalıcı hata (bozuk JSON → asla düzelmez)

### Basamak 2 — DLQ / karantina (~30 dk)
- [ ] `MessagingTopology`'ye sabitler: `DeadLetterExchange = "ticketreserve.dead-letter"`, `DeadLetterQueue = "notifications.dead-letter"`
- [ ] Consumer `ExecuteAsync`: DLX declare (**Fanout** — karantinada seçicilik anlamsız, etikete bakma) + DLQ declare + bind (`routingKey: ""`)
- [ ] Ana kuyruk declare'ine `arguments: { "x-dead-letter-exchange": DeadLetterExchange }`
- [ ] ⚠️ **TUZAK:** mevcut `notifications` kuyruğu argümansız yaratıldı — farklı argümanla declare `PRECONDITION_FAILED` verir.
      Uygulamayı başlatmadan önce kuyruğu management UI'dan SİL (boş, kayıp yok); consumer doğru argümanla yeniden yaratır.
      (Prod'da bu dert policy'lerle çözülür — argüman kuyruğa değil policy'ye konur.)
- Test: bozuk gövdeli mesaj bas → log'da BİR kez "karantinaya taşınıyor" → `notifications` boş → DLQ'da 1 mesaj → `x-death` header'ını incele

### Basamak 3 — Deneme sayacı "3 dene, sonra DLQ" (~1-1,5 saat, kavramca en ağırı)
- AMQP mesajda deneme sayacı tutmaz (sadece `redelivered` bayrağı) → turnike deseni:
  ana kuyruk → hata → TTL'li retry kuyruğuna dead-letter → TTL dolunca ana kuyruğa geri dead-letter → her turda `x-death` sayısı +1 → handler'da oku, eşik aşılınca gerçek DLQ'ya
- İki ek kuyruk + iki DLX bağlantısı + header okuma. Mülakat değeri: turnike desenini ANLATABİLMEK.

## Diğer bilinen açıklar (bilinçli ertelenmiş — "gerektiğinde şöyle çözerim" cevaplarıyla)

- [ ] **Dispatcher publish-retry sınırı:** `Error` yazılan satırın `ProcessedOnUtc`'u null kaldığı için her turda yeniden denenir (producer tarafı sonsuz retry) → `RetryCount` kolonu + eşik
- [ ] **Outbox temizliği:** işlenmiş satırlar sonsuz birikir → retention job ("X günden eski işlenmişleri sil")
- [ ] **Yatay ölçekte dispatcher:** N replica = N postacı aynı satırları okur → `FOR UPDATE SKIP LOCKED` (Adım 9'da patlayacak mayın)
- [ ] **İş-idempotency sınırı:** idempotency garantisi Inbox commit'ine kadar; iş (mail) ile commit arası çökmede iş tekrarlanır → gerçek mail'de sağlayıcıya idempotency key
- [ ] **docs/interview-stories.md:** Adım 4 + Adım 5 pitch'leri hâlâ eklenmedi

## Çalışma notları nerede
- Mülakat Q&A (Redis lock / RabbitMQ / Outbox-Inbox, koda dayalı S1-S29): Notion'a aktarıldı (sohbet 2026-07-19)
- Adım 5 bölüm 1-2 notları: Notion
