#!/usr/bin/env bash
set -Eeuo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT_DIR"

ENVIRONMENT="${1:?Kullanim: ./rollback-environment.sh <production|staging>}"
ENV_FILE=".env.server"
COMPOSE_FILE="compose.shared-server.yml"
active_file=".active-${ENVIRONMENT}-slot"

if [[ "$ENVIRONMENT" != "production" && "$ENVIRONMENT" != "staging" ]]; then
  echo "Ortam production veya staging olmalidir." >&2
  exit 1
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

sed -i "s|api-${ENVIRONMENT}-${current}:5118|api-${ENVIRONMENT}-${target}:5118|" Caddyfile.roomora
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" exec -T gateway \
  caddy reload --config /etc/caddy/Caddyfile
printf '%s\n' "$target" > "$active_file"
sleep 10
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" stop "api-${ENVIRONMENT}-${current}"

echo "${ENVIRONMENT} trafigi ${target} yuvasina geri alindi."
