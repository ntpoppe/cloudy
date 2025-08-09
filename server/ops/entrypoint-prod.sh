#!/usr/bin/env bash
set -euo pipefail

if [ "${ASPNETCORE_ENVIRONMENT:-}" = "Production" ] && [ "${PROD_GUARD:-NO}" != "YES" ]; then
  echo "Refusing to start: PROD_GUARD is not YES (current: '${PROD_GUARD:-unset}')."
  echo "Set PROD_GUARD=YES in your .env.prod on the server to allow prod."
  exit 42
fi

exec ./Cloudy.API.dll
