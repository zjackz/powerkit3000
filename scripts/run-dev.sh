#!/bin/bash

# 设置颜色变量
GREEN='\033[0;32m'
NC='\033[0m' # No Color

# 设置脚本在任何命令失败时退出
set -e

echo -e "${GREEN}Starting services with Docker Compose...${NC}"
# 使用 docker-compose 启动所有服务
docker-compose up --build

echo -e "\n${GREEN}Services stopped.${NC}"
