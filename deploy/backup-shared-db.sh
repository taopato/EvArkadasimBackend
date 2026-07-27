#!/usr/bin/env bash
set -Eeuo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT_DIR"

ENV_FILE=".env.server"
COMPOSE_FILE="compose.shared-server.yml"
backup_path="$(sed -n 's/^ROOMORA_BACKUP_PATH=//p' "$ENV_FILE" | tail -n 1)"
backup_path="${backup_path:-/opt/roomora/backups}"

db_container="$(docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" ps -q db)"
if [[ -z "$db_container" || "$(docker inspect --format '{{.State.Running}}' "$db_container")" != "true" ]]; then
  echo "Veritabani calismiyor; yedek alinamadi." >&2
  exit 1
fi

db_password="$(
  docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" exec -T db \
    printenv MSSQL_SA_PASSWORD
)"
timestamp="$(date -u +%Y%m%d_%H%M%S)"

for key in ROOMORA_PRODUCTION_DB_NAME ROOMORA_STAGING_DB_NAME; do
  database_name="$(sed -n "s/^${key}=//p" "$ENV_FILE" | tail -n 1)"
  filename="${database_name}_${timestamp}.bak"
  docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" exec -T db \
    /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$db_password" -C -b \
    -Q "IF DB_ID(N'${database_name}') IS NOT NULL
        BACKUP DATABASE [${database_name}]
        TO DISK = N'/var/opt/mssql/backup/${filename}'
        WITH COPY_ONLY, CHECKSUM"
done

find "$backup_path" -type f -name 'Roomora*.bak' -mtime +14 -delete
echo "Production ve staging veritabani yedekleri tamamlandi."
