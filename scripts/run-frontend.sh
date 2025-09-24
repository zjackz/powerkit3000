#!/usr/bin/env bash

set -euo pipefail

GREEN='\033[0;32m'
YELLOW='\033[0;33m'
NC='\033[0m'

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
APP_DIR="$ROOT_DIR/frontend/tradeforge-suite"

if [[ ! -d "$APP_DIR" ]]; then
  echo -e "${YELLOW}[run-frontend] 未找到目录: $APP_DIR${NC}" >&2
  exit 1
fi

echo -e "${GREEN}[run-frontend] 切换到 $APP_DIR${NC}"
cd "$APP_DIR"

if [[ ! -d node_modules ]]; then
  echo -e "${YELLOW}[run-frontend] 检测到缺少 node_modules，执行 npm install...${NC}"
  npm install
fi

echo -e "${GREEN}[run-frontend] 启动 Vite 开发服务器...${NC}"
npm run dev -- "$@"
