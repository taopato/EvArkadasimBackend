#!/usr/bin/env bash
set -Eeuo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT_DIR"

exec 9> .deployment.lock
if ! flock -w 900 9; then
  echo "Baska bir Roomora dagitimi tamamlanmadi." >&2
  exit 1
fi

ENVIRONMENT="${1:?Kullanim: ./rollback-environment.sh <production|staging>}"
ENV_FILE=".env.server"
COMPOSE_FILE="compose.shared-server.yml"
active_file=".active-${ENVIRONMENT}-slot"

if [[ "$ENVIRONMENT" != "production" && "$ENVIRONMENT" != "staging" ]]; then
  echo "Ortam production veya staging olmalidir." >&2
  exit 1
fi

if [[ "$ENVIRONMENT" == "production" ]]; then
  domain="${ROOMORA_PRODUCTION_DOMAIN:-api.roomora.com}"
else
  domain="${ROOMORA_STAGING_DOMAIN:-testapi.roomora.com}"
fi

current="$(tr -d '[:space:]' < "$active_file")"
if [[ "$current" == "blue" ]]; then
  target="green"
elif [[ "$current" == "green" ]]; then
  target="blue"
else
  echo "Aktif yuva bilgisi gecersiz." >&2
  exit 1
fi

service="api-${ENVIRONMENT}-${target}"
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" start "$service"
container_id="$(docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" ps -q "$service")"

for attempt in $(seq 1 30); do
  status="$(docker inspect --format '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}' "$container_id")"
  [[ "$status" == "healthy" ]] && break
  [[ "$attempt" == "30" ]] && { echo "Rollback yuvasi hazir degil." >&2; exit 1; }
  sleep 2
done

sed -i -E "s#api-${ENVIRONMENT}-(blue|green):5118#api-${ENVIRONMENT}-${target}:5118#" Caddyfile.roomora
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" exec -T gateway \
  caddy validate --config - --adapter caddyfile < Caddyfile.roomora
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" exec -T gateway \
  caddy reload --config - --adapter caddyfile < Caddyfile.roomora

if ! docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" exec -T gateway \
  wget -qO- --header="Host: ${domain}" http://127.0.0.1/health > /dev/null; then
  sed -i -E "s#api-${ENVIRONMENT}-(blue|green):5118#api-${ENVIRONMENT}-${current}:5118#" Caddyfile.roomora
  docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" exec -T gateway \
    caddy reload --config - --adapter caddyfile < Caddyfile.roomora
  docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" stop "$service"
  echo "Rollback saglik kontrolu basarisiz; mevcut yuva korunuyor." >&2
  exit 1
fi

printf '%s\n' "$target" > "$active_file"
sleep 10
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" stop "api-${ENVIRONMENT}-${current}"

echo "${ENVIRONMENT} trafigi ${target} yuvasina geri alindi."
