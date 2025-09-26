#!/usr/bin/env bash

set -euo pipefail

GREEN='\033[0;32m'
YELLOW='\033[0;33m'
NC='\033[0m'

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
APP_DIR="$ROOT_DIR/frontend/tradeforge-suite-next"

if [[ ! -d "$APP_DIR" ]]; then
  echo -e "${YELLOW}[dev] 未找到目录: $APP_DIR${NC}" >&2
  exit 1
fi

export DOTNET_ENVIRONMENT="${DOTNET_ENVIRONMENT:-Company}"
export ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-$DOTNET_ENVIRONMENT}"

echo -e "${GREEN}[dev] 切换到 $APP_DIR${NC}"
cd "$APP_DIR"

if [[ ! -d node_modules ]]; then
  echo -e "${YELLOW}[dev] 检测到缺少 node_modules，执行 npm install...${NC}"
  npm install
fi

echo -e "${GREEN}[dev] 启动 Next.js 开发服务器...${NC}"
npm run dev -- "$@"
