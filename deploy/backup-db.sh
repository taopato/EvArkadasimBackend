#!/usr/bin/env bash
set -Eeuo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT_DIR"

ENV_FILE=".env.production"
set -a
source "$ENV_FILE"
set +a

db_container="$(docker compose --env-file "$ENV_FILE" -f compose.production.yml ps -q db)"
if [[ -z "$db_container" || "$(docker inspect --format '{{.State.Running}}' "$db_container")" != "true" ]]; then
  echo "Veritabani henuz calismiyor; ilk kurulum yedegi atlandi."
  exit 0
fi

database_exists="$(
  docker compose --env-file "$ENV_FILE" -f compose.production.yml exec -T db \
    /opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P "$MSSQL_SA_PASSWORD" \
    -C \
    -h -1 \
    -W \
    -Q "SET NOCOUNT ON; SELECT CASE WHEN DB_ID(N'${ROOMORA_DB_NAME}') IS NULL THEN 0 ELSE 1 END"
)"

if [[ "$(echo "$database_exists" | tr -d '[:space:]')" != "1" ]]; then
  echo "Veritabani henuz olusmamis; ilk kurulum yedegi atlandi."
  exit 0
fi

timestamp="$(date -u +%Y%m%d_%H%M%S)"
filename="${ROOMORA_DB_NAME}_${timestamp}.bak"

docker compose --env-file "$ENV_FILE" -f compose.production.yml exec -T db \
  /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P "$MSSQL_SA_PASSWORD" \
  -C \
  -Q "BACKUP DATABASE [${ROOMORA_DB_NAME}] TO DISK = N'/var/opt/mssql/backup/${filename}' WITH COPY_ONLY, COMPRESSION, CHECKSUM"

find "${ROOMORA_BACKUP_PATH:-/opt/roomora/backups}" \
  -type f \
  -name "${ROOMORA_DB_NAME}_*.bak" \
  -mtime +14 \
  -delete

echo "Yedek hazir: ${filename}"
