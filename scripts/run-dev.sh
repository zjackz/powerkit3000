#!/bin/bash

# 设置颜色变量
GREEN='\033[0;32m'
NC='\033[0m' # No Color

# 设置脚本在任何命令失败时退出
set -e

echo -e "${GREEN}Starting frontend development server...${NC}"

# Navigate to the frontend directory and run the application
(cd frontend && npm install && npm run dev)

echo -e "\n${GREEN}Frontend development server stopped.${NC}"
