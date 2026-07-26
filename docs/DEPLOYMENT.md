# Roomora Gelistirme ve Canliya Alma

## Akis

1. Degisiklik local Docker ortaminda test edilir.
2. Kod bir feature branch'e pushlanir ve pull request acilir.
3. CI, .NET build ve Docker image build kontrollerini yapar.
4. `main` branch'e birlestirildiginde GHCR'a commit SHA etiketiyle image yazilir.
5. Production deploy workflow'u manuel onayla baslatilir.
6. Sunucu migration'i kontrollu calistirir ve bos blue/green yuvasini acilir.
7. Health check basariliysa Caddy trafigi yeni yuvaya kesintisiz aktarir.
8. Sorunda `rollback.sh` onceki container'a geri doner.

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

## Production Ilk Kurulum

Sunucuda `/opt/roomora/deploy` dizinine `deploy` klasorunun icerigi kopyalanir.
`.env.production.example`, `.env.production` adi ile kopyalanir ve gercek
secret degerleri girilir. Bu dosya Git'e eklenmez.

```bash
chmod +x deploy.sh rollback.sh backup-db.sh restore-db.sh
./deploy.sh <backend-image-tag>
```

Firewall'da yalnizca `80` ve `443` herkese acilir. SSH, IP allowlist veya VPN
ile sinirlanir. SQL Server `1433` portu production hostuna publish edilmez.

### Mevcut SSMS Veritabanini Ilk Kez Tasima

Local SQL Server'dan `COPY_ONLY` tam `.bak` yedegi alin. Dosyayi SSH/SCP ile
sunucudaki `/opt/roomora/backups` dizinine aktarip, ilk production deploy'undan
once su komutu calistirin:

```bash
cd /opt/roomora/deploy
./restore-db.sh RoomoraDb_Initial.bak
./deploy.sh <backend-image-tag>
```

`restore-db.sh`, aktif production yuvasi olustuktan sonra calismayi reddeder.
Canli veritabani degisimi yedek, bakim plani ve ayrica onay gerektirir.

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
./rollback.sh
```

Rollback kodu geri alir. Veritabani migration'ini otomatik geri almaz. Bu
nedenle production migration'lari geriye uyumlu tasarlanir ve her deploy
oncesi yedek alinir.

## Otomatik Yedek

Cron ornegi:

```cron
15 3 * * * /opt/roomora/deploy/backup-db.sh >> /var/log/roomora-backup.log 2>&1
```

Yedeklerin sunucu disinda sifreli ikinci bir konuma kopyalanmasi gerekir.

## GitHub Secrets

Production environment altinda:

- `SERVER_HOST`
- `SERVER_USER`
- `SERVER_SSH_KEY`
- `SERVER_KNOWN_HOSTS`
- `GHCR_USER`
- `GHCR_READ_TOKEN`

Production environment icin required reviewer acilmasi onerilir. Boylece
`main` branch'e push yapmak tek basina canli deploy baslatmaz.
