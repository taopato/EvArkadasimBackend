# EvArkadasim Backend

Clean Architecture yapısında .NET backend API.

## Çalıştırma

```bash
dotnet restore
dotnet build
dotnet run --project EvArkadasim.API
```

## Ana Modüller

- `EvArkadasim.API`: controller ve startup
- `Application`: command/query handler katmanı
- `Persistence`: EF Core ve repository implementasyonları
- `Domain`: entity ve enum tanımları

## Aktif İş Alanları

- Auth ve Google login
- House oluşturma ve üye/davet akışı
- Expense oluşturma, güncelleme, silme
- Ledger tabanlı borç/alacak hesapları
- Payment oluşturma ve onaylama

## Notlar

- Borç özeti `LedgerLine` kayıtları üzerinden hesaplanır.
- Google login için `appsettings.json` içindeki Google client ID yapılandırması gerekir.
- Build şu an başarılıdır; yalnız paket uyumluluğu ve AutoMapper güvenlik uyarıları bulunmaktadır.
