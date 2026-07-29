# Roomora Infrastructure Handoff

Last verified: 2026-07-27

This document is the source of truth for an independent infrastructure and
release-readiness review. It intentionally contains no passwords, tokens, or
private SSH key material.

## Current Verdict

The backend infrastructure is running and healthy inside the server, but the
application is NOT ready for a public App Store or Google Play release yet.

Public release blockers:

1. Public API health and authentication flows must pass through the existing
   `control.builtwhys.space` HTTPS endpoint before release.
2. The server has placeholder SMTP configuration. Email verification and
   password reset cannot be considered operational.
3. Real Google iOS/Android OAuth client IDs and Apple release credentials have
   not been configured and verified.
4. Mobile OTA/release automation is intentionally disabled until the first
   signed store builds and an Expo access token exist.
5. SQL backups currently remain on the same server. Offsite encrypted backup
   storage is not configured.

Do not approve a public mobile release until these five items are resolved and
the public HTTPS smoke tests pass.

## Implemented Architecture

- Server: Ubuntu host at `65.109.139.24`
- SSH alias used by the owner: `benim-sunucum`
- Runtime: Docker Engine and Docker Compose
- Existing public reverse proxy: BuiltWhys Caddy on ports 80 and 443
- Roomora internal gateway: Caddy container on the shared Docker network
- Production API: blue/green containers
- Staging API: blue/green containers
- Database: one private SQL Server container with two separate databases
- OCR: one shared stateless OCR container
- SQL Server port 1433 is not published to the internet
- Production and staging uploads use separate Docker volumes

Environment mapping:

| Git branch | Environment | API domain | Database |
| --- | --- | --- | --- |
| `development` | staging | `control.builtwhys.space/roomora-testapi` | `RoomoraStagingDb` |
| `main` | production | `control.builtwhys.space/roomora-api` | `RoomoraDb` |

## Deployment Behavior

1. A push runs Backend CI and publishes a SHA-tagged image to GHCR.
2. A successful image workflow triggers `Deploy Server Environment`.
3. `development` deploys only staging.
4. `main` deploys only production.
5. Deployment takes a database backup and runs migrations separately.
6. The inactive blue/green slot starts and must become healthy.
7. Gateway routing is loaded directly into Caddy.
8. A Host-based `/health` request must pass before the old slot stops.
9. GitHub concurrency and a server-side `flock` prevent simultaneous releases.
10. Rollback starts the previous slot, verifies it through the gateway, then
    stops the faulty slot.

GitHub environments:

- `backend-staging`
- `backend-production`

GitHub secrets configured:

- `SERVER_HOST`
- `SERVER_USER`
- `SERVER_SSH_KEY`
- `SERVER_KNOWN_HOSTS`

The CI deploy key is dedicated to the `roomora-deploy` Linux user. The owner's
existing personal SSH private key was not read or copied.

## Database And Backups

The local `EvArkadasimDb` database was backed up with COPY_ONLY, COMPRESSION,
and CHECKSUM, transferred to the server, checksum-verified, and restored as:

- `RoomoraDb`
- `RoomoraStagingDb`

Remote backup command:

```bash
cd /opt/roomora/deploy
./backup-shared-db.sh
```

Installed cron:

```cron
15 3 * * * cd /opt/roomora/deploy && ./backup-shared-db.sh >> /opt/roomora/backups/backup.log 2>&1
```

Backups are stored in `/opt/roomora/backups`. An independent reviewer should
require an encrypted offsite copy before approving production.

## Mobile Environment Mapping

Expo/EAS project: `@takopato4/ev-arkadasim`

The technical Expo slug remains `ev-arkadasim` for project compatibility. The
user-facing application name and package identity are Roomora.

Configured EAS public variables:

| EAS environment | `EXPO_PUBLIC_API_URL` |
| --- | --- |
| production | `https://control.builtwhys.space/roomora-api` |
| preview | `https://control.builtwhys.space/roomora-testapi` |
| development | `https://control.builtwhys.space/roomora-testapi` |

Bundle identifiers:

- iOS: `com.taopato.roomora`
- Android: `com.taopato.roomora`

## Verification Already Completed

- Backend CI passed on `main` and `development`.
- GHCR image publishing passed on both branches.
- Automatic production deployment passed.
- Automatic staging deployment passed.
- Production internal gateway health returned database `available`.
- Staging internal gateway health returned database `available`.
- Blue/green deployment was exercised.
- Staging rollback was exercised and then redeployed to the latest image.
- Deployment route validation caught and prevented a broken target.
- Firewall allows SSH, HTTP, and HTTPS; SQL Server is not exposed.
- Mobile domain tests passed.
- Expo Doctor passed 20/20 checks.
- Android Expo export passed.
- iOS Expo export passed.
- Mobile GitHub Frontend CI passed.

## Independent Review Commands

Do not print `.env.server`, private keys, GitHub secret values, or database
passwords during review.

Server container and health review:

```bash
ssh benim-sunucum
cd /opt/roomora/deploy
docker compose --env-file .env.server -f compose.shared-server.yml ps
docker compose --env-file .env.server -f compose.shared-server.yml exec -T gateway \
  wget -qO- --header='Host: api.roomora.builtwhys.space' http://127.0.0.1/health
docker compose --env-file .env.server -f compose.shared-server.yml exec -T gateway \
  wget -qO- --header='Host: testapi.roomora.builtwhys.space' http://127.0.0.1/health
```

Public HTTPS review:

```powershell
curl.exe -fsS https://control.builtwhys.space/roomora-api/health
curl.exe -fsS https://control.builtwhys.space/roomora-testapi/health
```

No additional DNS records are required. Both APIs share the existing
`control.builtwhys.space` certificate and are separated by path routing.

Repository review:

```powershell
cd C:\EvArkadasimProje\GitHubRepos\EvArkadasimBackend
git status --short --branch
gh run list --repo taopato/RoomoraBackend

cd C:\EvArkadasimProje\GitHubRepos\EvArkadasimMobile
npm run release:check
npx eas-cli env:list --environment production
npx eas-cli env:list --environment preview
npx eas-cli env:list --environment development
```

## Required Public Release Gate

An independent reviewer should approve release only after all of the following
are demonstrated:

- Both public HTTPS health URLs return HTTP 200.
- A real user can register, verify email, log in, reset password, and delete
  the account against production.
- Google login works in signed Android and iOS builds.
- Sign in with Apple works in the signed iOS build.
- Receipt upload/OCR works through the public API.
- Profile and house image uploads remain accessible after a container switch.
- Database backup restore is tested in staging.
- TestFlight and Play Internal Testing builds complete their end-to-end flows.
- Privacy policy, terms, support, and account deletion URLs are publicly
  reachable and use final production content.
- Crash reporting, monitoring, and an offsite backup destination are selected.

Related documentation:

- `docs/DEPLOYMENT.md`
- Mobile repository: `docs/MOBILE_RELEASE_FLOW.md`
- Mobile repository: `docs/IMPLEMENTATION_NOTES.md`
