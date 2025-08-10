# 构建阶段
FROM node:20-alpine AS builder
WORKDIR /app
# 设置 npm 镜像源
RUN npm config set registry https://registry.npmmirror.com
# 复制 package 文件
COPY frontend/package*.json ./
# 安装依赖
RUN npm ci || npm install
# 复制源代码
COPY frontend/ .
# 构建应用
RUN npm run build
# 生产阶段
FROM node:20-alpine AS runner
WORKDIR /app
ENV NODE_ENV production
RUN addgroup -g 1001 -S nodejs && \
    adduser -S nextjs -u 1001
# 复制必要文件
COPY --from=builder /app/public ./public
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static
USER nextjs
EXPOSE 3000
ENV PORT 3000
ENV HOSTNAME "0.0.0.0"
CMD ["node", "server.js"]
