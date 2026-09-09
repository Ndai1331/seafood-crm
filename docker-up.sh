#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")"

if [ -f .env ]; then
  set -a
  # shellcheck disable=SC1091
  source .env
  set +a
fi

docker compose up --build -d

echo "Waiting for Postgres..."
for i in $(seq 1 40); do
  if docker compose exec -T postgres pg_isready -U "${POSTGRES_USER:-seafood}" >/dev/null 2>&1; then
    break
  fi
  sleep 2
done

echo
echo "Seafood CRM is up."
echo "  UI:  http://localhost:8080"
echo "  API: http://localhost:5093/swagger"
echo "  Admin: ${BOOTSTRAP_ADMIN_USER:-admin} / ${BOOTSTRAP_ADMIN_PASSWORD:-Admin@123}"
