# Roomora Infrastructure Handoff

Last verified: 2026-07-30

This document is the source of truth for an independent infrastructure and
release-readiness review. It intentionally contains no passwords, tokens, or
private SSH key material.

## Current Verdict

The backend infrastructure is running and healthy inside the server, but the
application is NOT ready for a public App Store or Google Play release yet.

Public release blockers:

1. Cloudflare DNS records for `api.takosware.com`, `testapi.takosware.com` and
   `control.takosware.com` must be created and their public HTTPS health checks
   must pass.
2. A transactional email provider must verify `takosware.com`; its SMTP
   credential must then be stored only in `/opt/roomora/deploy/.env.server`.
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
| `development` | staging | `testapi.takosware.com` | `RoomoraStagingDb` |
| `main` | production | `api.takosware.com` | `RoomoraDb` |

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
| production | `https://api.takosware.com` |
| preview | `https://testapi.takosware.com` |
| development | `https://testapi.takosware.com` |

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
curl.exe -fsS https://api.takosware.com/health
curl.exe -fsS https://testapi.takosware.com/health
```

The old `control.builtwhys.space` path routes remain only as a temporary
compatibility fallback. Store builds use the Takosware hosts.

## Control Center

- Public host: `https://control.takosware.com`
- Authentication: the owner account is configured through environment
  variables; no plaintext password is stored in source control.
- The Roomora page checks production API/database, staging API/database and OCR.
- The control center has network-only access to Roomora services. SQL Server
  and the Docker socket are not exposed to the panel.

## Authentication And Email

- Access tokens are short lived.
- Refresh tokens are random, stored as SHA-256 hashes in SQL Server, rotated on
  use and valid for 90 days unless revoked.
- Mobile refresh tokens use native secure storage.
- Logout revokes the active refresh token.
- Verification, password reset, invitation and account deletion emails use the
  responsive Roomora/Takosware template.
- `destek@takosware.com` is routed by Cloudflare Email Routing to the verified
  `ttarikcetinturk@gmail.com` owner mailbox. Incoming routing is free and
  independent from outbound SMTP.
- Outbound transactional email is active on staging and production through the
  Resend Free SMTP relay:
  `smtp.resend.com:587`, username `resend`, sender
  `Roomora <bildirim@takosware.com>`.
- `takosware.com` and its generated DKIM/SPF records are verified by Resend.
  Store the Resend API key only as `SMTP_PASSWORD` in
  `/opt/roomora/deploy/.env.server`; never commit it.

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
