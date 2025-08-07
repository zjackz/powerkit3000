#!/bin/bash

# 设置颜色变量
GREEN='\033[0;32m'
NC='\033[0m' # No Color

# 设置脚本在任何命令失败时退出
set -e

echo -e "${GREEN}Starting .NET Backend API in the background...${NC}"
# 启动后端 API，并将输出重定向到日志文件
dotnet run --project src/AmazonTrends.WebApp/AmazonTrends.WebApp.csproj > logs/backend-dev.log 2>&1 &
BACKEND_PID=$!
echo "Backend API started with PID: $BACKEND_PID"

# 等待几秒钟，确保后端有足够的时间启动
sleep 5

echo -e "\n${GREEN}Starting Next.js Frontend Development Server...${NC}"
# 进入前端项目目录并启动开发服务器
cd src/amazontrends-adminui
npm run dev

# 当 `npm run dev` 进程结束时（例如，通过 Ctrl+C），杀死后端进程
echo -e "\n${GREEN}Frontend server stopped. Shutting down backend API...${NC}"
kill $BACKEND_PID
echo "Backend API shut down."
