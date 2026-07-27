#!/usr/bin/env bash
set -Eeuo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT_DIR"

BACKUP_NAME="${1:?Kullanim: ./restore-shared-db.sh <backup.bak> <database-name>}"
DATABASE_NAME="${2:?Kullanim: ./restore-shared-db.sh <backup.bak> <database-name>}"
ENV_FILE=".env.server"
COMPOSE_FILE="compose.shared-server.yml"

if [[ "$BACKUP_NAME" != "$(basename "$BACKUP_NAME")" || "$BACKUP_NAME" != *.bak ]]; then
  echo "Gecersiz yedek dosyasi." >&2
  exit 1
fi

backup_path="$(sed -n 's/^ROOMORA_BACKUP_PATH=//p' "$ENV_FILE" | tail -n 1)"
backup_path="${backup_path:-/opt/roomora/backups}"
if [[ ! -f "${backup_path}/${BACKUP_NAME}" ]]; then
  echo "Yedek dosyasi bulunamadi." >&2
  exit 1
fi

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d db
db_password="$(
  docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" exec -T db \
    printenv MSSQL_SA_PASSWORD
)"
file_list="$(
  docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" exec -T db \
    /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$db_password" -C -b -h -1 -W -s "|" \
    -Q "RESTORE FILELISTONLY FROM DISK = N'/var/opt/mssql/backup/${BACKUP_NAME}'"
)"
data_logical="$(printf '%s\n' "$file_list" | awk -F'|' '$3 ~ /^[[:space:]]*D[[:space:]]*$/ {gsub(/^[ \t]+|[ \t]+$/, "", $1); print $1; exit}')"
log_logical="$(printf '%s\n' "$file_list" | awk -F'|' '$3 ~ /^[[:space:]]*L[[:space:]]*$/ {gsub(/^[ \t]+|[ \t]+$/, "", $1); print $1; exit}')"

if [[ -z "$data_logical" || -z "$log_logical" ]]; then
  echo "Yedek mantiksal dosyalari okunamadi." >&2
  exit 1
fi

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" exec -T db \
  /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$db_password" -C -b \
  -Q "RESTORE DATABASE [${DATABASE_NAME}]
      FROM DISK = N'/var/opt/mssql/backup/${BACKUP_NAME}'
      WITH MOVE N'${data_logical}' TO N'/var/opt/mssql/data/${DATABASE_NAME}.mdf',
           MOVE N'${log_logical}' TO N'/var/opt/mssql/data/${DATABASE_NAME}_log.ldf',
           REPLACE, RECOVERY, CHECKSUM"

echo "${DATABASE_NAME} veritabani geri yuklendi."
