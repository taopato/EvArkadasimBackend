#!/usr/bin/env bash
set -Eeuo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT_DIR"

ENV_FILE=".env.production"
ACTIVE_FILE=".active-slot"
current="$(tr -d '[:space:]' < "$ACTIVE_FILE")"

if [[ "$current" == "blue" ]]; then
  target="green"
elif [[ "$current" == "green" ]]; then
  target="blue"
else
  echo "Aktif yuva bilgisi gecersiz." >&2
  exit 1
fi

docker compose --env-file "$ENV_FILE" -f compose.production.yml start "api-${target}"
container_id="$(docker compose --env-file "$ENV_FILE" -f compose.production.yml ps -q "api-${target}")"

for attempt in $(seq 1 20); do
  status="$(docker inspect --format '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}' "$container_id")"
  [[ "$status" == "healthy" ]] && break
  [[ "$attempt" == "20" ]] && { echo "Rollback yuvasi hazir degil." >&2; exit 1; }
  sleep 2
done

domain="$(grep '^ROOMORA_DOMAIN=' "$ENV_FILE" | cut -d= -f2-)"
sed -i "s|reverse_proxy api-${current}:5118|reverse_proxy api-${target}:5118|" Caddyfile
docker compose --env-file "$ENV_FILE" -f compose.production.yml exec -T caddy \
  caddy reload --config /etc/caddy/Caddyfile
printf '%s\n' "$target" > "$ACTIVE_FILE"
sleep 10
docker compose --env-file "$ENV_FILE" -f compose.production.yml stop "api-${current}"

echo "Trafik ${target} yuvasina geri alindi."
