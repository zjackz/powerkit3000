# 使用官方 Node.js 镜像作为构建阶段的基础镜像
FROM node:20-alpine AS builder

# 设置工作目录
WORKDIR /app

# 复制 package.json 和 package-lock.json
COPY frontend/package.json frontend/package-lock.json ./

# 安装依赖
RUN npm install

# 复制前端所有文件
COPY frontend/ ./

# 构建 Next.js 应用
RUN npm run build

# 使用一个更小的镜像作为生产环境的基础镜像
FROM node:20-alpine AS runner

WORKDIR /app

# 设置环境变量，告诉 Next.js 在生产模式下运行
ENV NODE_ENV production

# 复制构建好的应用文件
COPY --from=builder /app/.next ./.next
COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app/public ./public
COPY --from=builder /app/package.json ./

# 暴露 Next.js 应用的默认端口
EXPOSE 3000

# 启动 Next.js 应用
CMD ["npm", "start"]
