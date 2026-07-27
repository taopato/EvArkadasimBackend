#!/usr/bin/env bash
set -Eeuo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT_DIR"

ENV_FILE=".env.production"
BACKUP_FILE="${1:?Kullanim: ./restore-db.sh <yedek-dosyasi.bak>}"
BACKUP_NAME="$(basename "$BACKUP_FILE")"

if [[ "$BACKUP_NAME" != "$BACKUP_FILE" || "$BACKUP_NAME" != *.bak ]]; then
  echo "Yedek dosyasi /opt/roomora/backups altinda bir .bak dosyasi olmalidir." >&2
  exit 1
fi

if [[ -f ".active-slot" ]]; then
  echo "Bu komut yalnizca ilk production kurulumundan once kullanilabilir." >&2
  exit 1
fi

database_name="$(sed -n 's/^ROOMORA_DB_NAME=//p' "$ENV_FILE" | tail -n 1)"
backup_path="$(sed -n 's/^ROOMORA_BACKUP_PATH=//p' "$ENV_FILE" | tail -n 1)"
backup_path="${backup_path:-/opt/roomora/backups}"

if [[ -z "$database_name" || ! -f "${backup_path}/${BACKUP_NAME}" ]]; then
  echo "Veritabani adi veya yedek dosyasi bulunamadi." >&2
  exit 1
fi

docker compose --env-file "$ENV_FILE" -f compose.production.yml up -d db
db_password="$(
  docker compose --env-file "$ENV_FILE" -f compose.production.yml exec -T db \
    printenv MSSQL_SA_PASSWORD
)"

file_list="$(
  docker compose --env-file "$ENV_FILE" -f compose.production.yml exec -T db \
    /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$db_password" -C -h -1 -W -s "|" \
    -Q "RESTORE FILELISTONLY FROM DISK = N'/var/opt/mssql/backup/${BACKUP_NAME}'"
)"

data_logical="$(printf '%s\n' "$file_list" | awk -F'|' '$3 ~ /^[[:space:]]*D[[:space:]]*$/ {gsub(/^[ \t]+|[ \t]+$/, "", $1); print $1; exit}')"
log_logical="$(printf '%s\n' "$file_list" | awk -F'|' '$3 ~ /^[[:space:]]*L[[:space:]]*$/ {gsub(/^[ \t]+|[ \t]+$/, "", $1); print $1; exit}')"

if [[ -z "$data_logical" || -z "$log_logical" ]]; then
  echo "Yedekteki veri ve log dosyalari okunamadi." >&2
  exit 1
fi

docker compose --env-file "$ENV_FILE" -f compose.production.yml exec -T db \
  /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$db_password" -C \
  -Q "RESTORE DATABASE [${database_name}]
      FROM DISK = N'/var/opt/mssql/backup/${BACKUP_NAME}'
      WITH MOVE N'${data_logical}' TO N'/var/opt/mssql/data/${database_name}.mdf',
           MOVE N'${log_logical}' TO N'/var/opt/mssql/data/${database_name}_log.ldf',
           REPLACE, RECOVERY, CHECKSUM"

echo "${BACKUP_NAME} yedegi ${database_name} veritabanina geri yuklendi."
