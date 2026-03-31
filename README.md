# Ev Arkadasim Backend

Ev Arkadasim'in backend servisi; kimlik dogrulama, ev ve uye yonetimi, harcama akislari, duzenli gider planlari, odemeler ve borc-alacak hesaplarini tek bir API altinda toplar.

Yapi, is kurallarini ve veri erisimini ayri katmanlarda tutan bir Clean Architecture duzeni uzerine kuruludur. Ama amac yalnizca teknik olarak duzgun olmak degil; urun tarafinda hizli ilerlerken bakimi da kolay tutmaktir.

## Sorumluluklar

- Auth ve JWT tabanli oturum yonetimi
- Ev olusturma, uye daveti ve grup akislari
- Harcama olusturma, guncelleme ve silme
- Duzensiz gider, duzenli gider ve taksitli plan senaryolari
- Ledger tabanli borc-alacak hesaplamalari
- Odeme ve onay mekanizmalari
- Fis, belge ve destekleyici servis akislari

## Teknoloji

- .NET 7
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- MediatR
- FluentValidation

## Yerel Calistirma

```bash
dotnet restore
dotnet build
dotnet run --project EvArkadasim.API
```

## Katmanlar

- `EvArkadasim.API`: controller, middleware ve uygulama girisi
- `Application`: command-query akislari ve is kurallari
- `Persistence`: veritabani, EF Core ve repository katmani
- `Domain`: entity ve enum tanimlari
- `Core`: ortak yardimci yapilar ve guvenlik bilesenleri

## Yaklasim

Bu repo, urun ihtiyaclarina gore buyuyen ama dagilmamaya calisan bir backend yapisi sunar. Kodun amaci yalnizca endpoint acmak degil; kullanicinin gercekte yasadigi senaryolari guvenilir sekilde tasimaktir.
