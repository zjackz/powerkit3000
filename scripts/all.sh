#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}" )/.." && pwd)"

cleanup() {
  trap - INT TERM EXIT
  if [[ -n "${CONSOLE_PID:-}" ]] && ps -p "$CONSOLE_PID" > /dev/null 2>&1; then
    kill "$CONSOLE_PID" 2>/dev/null || true;
  fi
  if [[ -n "${API_PID:-}" ]] && ps -p "$API_PID" > /dev/null 2>&1; then
    kill "$API_PID" 2>/dev/null || true;
  fi
  if [[ -n "${FRONTEND_PID:-}" ]] && ps -p "$FRONTEND_PID" > /dev/null 2>&1; then
    kill "$FRONTEND_PID" 2>/dev/null || true;
  fi
}

trap cleanup INT TERM EXIT

echo "[all.sh] Building solution (pk3000.sln)..."
(
  cd "$ROOT_DIR"
  dotnet build pk3000.sln
)
echo "[all.sh] Build completed."

echo "[all.sh] Starting console app..."
(
  cd "$ROOT_DIR"
  DOTNET_ENVIRONMENT=${DOTNET_ENVIRONMENT:-Home} \
  ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-$DOTNET_ENVIRONMENT} \
  dotnet run --no-build --project backend/pk.consoleapp/pk.consoleapp.csproj
) &
CONSOLE_PID=$!

echo "[all.sh] Starting API..."
(
  cd "$ROOT_DIR"
  DOTNET_ENVIRONMENT=${DOTNET_ENVIRONMENT:-Home} \
  ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-$DOTNET_ENVIRONMENT} \
  dotnet run --no-build --project backend/pk.api/pk.api.csproj
) &
API_PID=$!

echo "[all.sh] Starting frontend..."
(
  cd "$ROOT_DIR/frontend/tradeforge-suite-next"
  PORT=${PORT:-3000} npm run dev
) &
FRONTEND_PID=$!

echo "[all.sh] All services started. Press Ctrl+C to stop."
wait
