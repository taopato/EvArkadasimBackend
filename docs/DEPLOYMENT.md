# Roomora Gelistirme ve Canliya Alma

## Akis

1. Degisiklik local Docker ortaminda test edilir.
2. Kod bir feature branch'e pushlanir ve pull request acilir.
3. CI, .NET build ve Docker image build kontrollerini yapar.
4. `development` push'u staging, `main` push'u production image'ini GHCR'a yazar.
5. Image yayini basarili olunca ilgili sunucu ortami otomatik dagitilir.
6. Sunucu migration'i kontrollu calistirir ve bos blue/green yuvasini acar.
7. Health check basariliysa Caddy trafigi yeni yuvaya kesintisiz aktarir.
8. Sorunda `rollback-environment.sh` onceki container'a geri doner.

## Roomora Sunucusu Branch Akisi

Roomora'nin paylasimli sunucu kurulumu `compose.shared-server.yml` kullanir:

- `development` push'u image yayinlandiktan sonra `testapi.roomora.builtwhys.space`
  staging ortaminda blue-green deploy edilir.
- `main` push'u image yayinlandiktan sonra `api.roomora.builtwhys.space` production
  ortaminda blue-green deploy edilir.
- Iki ortam ayri `RoomoraDb` ve `RoomoraStagingDb` veritabanlarini kullanir.
- SQL Server ve OCR altyapisi kaynak kullanimi icin ortaktir; uygulama
  verileri ve yuklenen dosyalar ortam bazinda ayridir.
- SQL Server disariya port acmaz. Dis trafik mevcut sunucu Caddy'sinden
  Roomora gateway'e, oradan aktif API slotuna gider.

Sunucuda:

```bash
cd /opt/roomora/deploy
./deploy-environment.sh staging <image-tag>
./deploy-environment.sh production <image-tag>
./rollback-environment.sh staging
./rollback-environment.sh production
./backup-shared-db.sh
```

GitHub Actions icin ayri `roomora-deploy` SSH kullanicisi kullanilir. Kisisel
SSH anahtari CI sistemine kopyalanmaz.

Kurulu alan adlari ve veritabanlari:

| Branch | Ortam | API | Veritabani |
| --- | --- | --- | --- |
| `development` | staging | `https://testapi.roomora.builtwhys.space` | `RoomoraStagingDb` |
| `main` | production | `https://api.roomora.builtwhys.space` | `RoomoraDb` |

Her iki alan adinin A kaydi `65.109.139.24` adresine yonelmelidir. DNS
yayildiktan sonra sunucudaki Caddy HTTPS sertifikasini otomatik alir.

## Local Docker

Docker Desktop calisirken backend repo kokunde:

```powershell
docker compose -f compose.local.yml up --build
```

Servisler:

- API: `http://localhost:5118`
- SQL Server: `localhost,14330` (yalnizca local makineye acik)
- OCR: `http://localhost:8008`

Kapatmak:

```powershell
docker compose -f compose.local.yml down
```

Verileri de tamamen silmek ancak bilerek sifirlamak istendiginde:

```powershell
docker compose -f compose.local.yml down --volumes
```

## Sunucu Konumu

- Compose ve betikler: `/opt/roomora/deploy`
- Gizli ayarlar: `/opt/roomora/deploy/.env.server`
- SQL yedekleri: `/opt/roomora/backups`
- Production yuklemeleri: `roomora_production_uploads` Docker volume
- Staging yuklemeleri: `roomora_staging_uploads` Docker volume

`.env.server` Git'e eklenmez ve dosya izni `600` olarak tutulur. SQL Server
`1433` portu hosta publish edilmez. Dis dunyaya yalnizca mevcut Caddy uzerinden
HTTP/HTTPS trafigi acilir.

### Mevcut SSMS Veritabanini Ilk Kez Tasima

Local SQL Server'dan `COPY_ONLY` tam `.bak` yedegi alin. Dosyayi SSH/SCP ile
sunucudaki `/opt/roomora/backups` dizinine aktarip, ilk production deploy'undan
once su komutu calistirin:

```bash
cd /opt/roomora/deploy
./restore-db.sh RoomoraDb_Initial.bak
./deploy.sh <backend-image-tag>
```

Paylasimli kurulumda geri yukleme komutu:

```bash
cd /opt/roomora/deploy
./restore-shared-db.sh production <yedek.bak>
./restore-shared-db.sh staging <yedek.bak>
```

Canli veritabani geri yuklemesi veri kaybina yol acabilecegi icin once mevcut
veritabani ayrica yedeklenmeli ve API trafigi kontrollu durdurulmalidir.

## Veritabani Degisiklikleri

Production API acilista migration calistirmaz. Deploy scripti yeni container
baslamadan once image'i `--migrate-only` ile calistirir.

Kesintisiz yayin icin migration'lar geriye uyumlu olmalidir:

1. Once yeni nullable kolon veya yeni tablo eklenir.
2. Yeni kod yayinlanir.
3. Veriler arkada doldurulur.
4. Eski kolon kaldirma gibi kirici degisiklik sonraki bir surume birakilir.

Tek deploy icinde kolon adini degistirmek veya zorunlu kolon eklemek eski
container'i bozabilir ve yapilmamalidir.

## Rollback

Uygulama hatasinda:

```bash
cd /opt/roomora/deploy
./rollback-environment.sh staging
./rollback-environment.sh production
```

Rollback kodu geri alir. Veritabani migration'ini otomatik geri almaz. Bu
nedenle production migration'lari geriye uyumlu tasarlanir ve her deploy
oncesi yedek alinir.

## Otomatik Yedek

Kurulu cron:

```cron
15 3 * * * cd /opt/roomora/deploy && ./backup-shared-db.sh >> /opt/roomora/backups/backup.log 2>&1
```

Bu yedekler ayni sunucudadir. Sunucu arizasina karsi S3, Backblaze B2 veya
baska bir sifreli uzak depoya ikinci kopya alinmasi zorunludur.

## GitHub Secrets

Repository secret'lari:

- `SERVER_HOST`
- `SERVER_USER`
- `SERVER_SSH_KEY`
- `SERVER_KNOWN_HOSTS`

GitHub environment'lari:

- `backend-staging`
- `backend-production`

Production icin daha sonra required reviewer acilabilir. Mevcut is akisi,
istenen branch modeline uygun olarak `main` push'unda otomatik production
dagitimi yapar.

## Canliya Cikmadan Once

1. `api.roomora.builtwhys.space` ve `testapi.roomora.builtwhys.space` A kayitlarini sunucu IP'sine yonelt.
2. `.env.server` icinde gercek SMTP bilgilerini tanimla ve iki API ortamını
   yeniden dagit. Aksi halde dogrulama ve sifre sifirlama e-postalari calismaz.
3. Google ve Apple oturum acma kimliklerini hem backend hem mobil EAS
   ortamlarinda tanimla.
4. Veritabani yedeklerini sunucu disindaki sifreli bir depoya kopyala.
