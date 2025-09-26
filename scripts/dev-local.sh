#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
API_PROJECT="$ROOT_DIR/backend/pk.api/pk.api.csproj"
FRONTEND_DIR="$ROOT_DIR/frontend/tradeforge-suite-next"

export DOTNET_ENVIRONMENT="${DOTNET_ENVIRONMENT:-Company}"
export ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-$DOTNET_ENVIRONMENT}"

cleanup() {
  echo "\n[dev-local] Stopping services..."
  kill 0 2>/dev/null || true
}

trap cleanup EXIT

if [[ ! -f "$API_PROJECT" ]]; then
  echo "[dev-local] 未找到 pk.api 工程: $API_PROJECT" >&2
  exit 1
fi

if [[ ! -d "$FRONTEND_DIR" ]]; then
  echo "[dev-local] 未找到前端目录: $FRONTEND_DIR" >&2
  exit 1
fi

echo "[dev-local] 启动 pk.api (环境: $DOTNET_ENVIRONMENT)..."
(
  cd "$ROOT_DIR"
  dotnet run --no-restore --project "$API_PROJECT"
) &

API_PID=$!

echo "[dev-local] 启动前端 Next.js 开发服务器..."
(
  cd "$FRONTEND_DIR"
  if [[ ! -d node_modules ]]; then
    npm install
  fi
  npm run dev
) &

FRONTEND_PID=$!

echo "[dev-local] 后端 PID: $API_PID, 前端 PID: $FRONTEND_PID"

wait
