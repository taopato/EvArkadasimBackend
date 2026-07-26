#!/usr/bin/env bash
set -Eeuo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT_DIR"

IMAGE_TAG="${1:?Kullanim: ./deploy.sh <image-tag>}"
ENV_FILE=".env.production"
ACTIVE_FILE=".active-slot"

if [[ ! -f "$ENV_FILE" ]]; then
  echo "$ENV_FILE bulunamadi." >&2
  exit 1
fi

current=""
if [[ -f "$ACTIVE_FILE" ]]; then
  current="$(tr -d '[:space:]' < "$ACTIVE_FILE")"
fi

if [[ "$current" == "blue" ]]; then
  target="green"
elif [[ "$current" == "green" ]]; then
  target="blue"
else
  target="blue"
fi

set_env_value() {
  local key="$1"
  local value="$2"
  if grep -q "^${key}=" "$ENV_FILE"; then
    sed -i "s|^${key}=.*|${key}=${value}|" "$ENV_FILE"
  else
    printf '%s=%s\n' "$key" "$value" >> "$ENV_FILE"
  fi
}

set_env_value ROOMORA_IMAGE_TAG "$IMAGE_TAG"

docker compose --env-file "$ENV_FILE" -f compose.production.yml pull db ocr "api-${target}" caddy
docker compose --env-file "$ENV_FILE" -f compose.production.yml up -d db ocr

echo "Veritabani migration adimi calisiyor..."
docker compose --env-file "$ENV_FILE" -f compose.production.yml run \
  --rm \
  --no-deps \
  -e Database__RunMigrations=true \
  "api-${target}" \
  --migrate-only

echo "Yeni ${target} yuvasi baslatiliyor..."
docker compose --env-file "$ENV_FILE" -f compose.production.yml up -d --no-deps "api-${target}"

container_id="$(docker compose --env-file "$ENV_FILE" -f compose.production.yml ps -q "api-${target}")"
for attempt in $(seq 1 30); do
  status="$(docker inspect --format '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}' "$container_id")"
  if [[ "$status" == "healthy" ]]; then
    break
  fi
  if [[ "$status" == "unhealthy" || "$status" == "exited" ]]; then
    docker logs "$container_id" --tail 100
    exit 1
  fi
  if [[ "$attempt" == "30" ]]; then
    echo "Yeni API saglik kontrolunu gecemedi." >&2
    docker logs "$container_id" --tail 100
    exit 1
  fi
  sleep 2
done

domain="$(grep '^ROOMORA_DOMAIN=' "$ENV_FILE" | cut -d= -f2-)"
cat > Caddyfile <<EOF
${domain} {
	encode zstd gzip
	reverse_proxy api-${target}:5118
	header {
		Strict-Transport-Security "max-age=31536000; includeSubDomains"
		X-Content-Type-Options "nosniff"
		X-Frame-Options "DENY"
		Referrer-Policy "strict-origin-when-cross-origin"
		-Server
	}
	log {
		output stdout
		format json
	}
}
EOF

docker compose --env-file "$ENV_FILE" -f compose.production.yml up -d caddy
docker compose --env-file "$ENV_FILE" -f compose.production.yml exec -T caddy \
  caddy reload --config /etc/caddy/Caddyfile

printf '%s\n' "$target" > "$ACTIVE_FILE"

if [[ -n "$current" && "$current" != "$target" ]]; then
  sleep 15
  docker compose --env-file "$ENV_FILE" -f compose.production.yml stop "api-${current}"
fi

echo "Roomora API ${IMAGE_TAG} etiketiyle ${target} yuvasinda yayinda."
