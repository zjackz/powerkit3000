# 构建阶段
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# 设置 NuGet 源
RUN dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org || true
# 复制解决方案和项目文件
COPY backend/*.sln ./
COPY backend/AmazonTrends.Domain/*.csproj ./AmazonTrends.Domain/
COPY backend/AmazonTrends.Application/*.csproj ./AmazonTrends.Application/
COPY backend/AmazonTrends.Infrastructure/*.csproj ./AmazonTrends.Infrastructure/
COPY backend/AmazonTrends.WebApp/*.csproj ./AmazonTrends.WebApp/
# 恢复依赖
RUN dotnet restore "AmazonTrends.WebApp/AmazonTrends.WebApp.csproj" --verbosity normal
# 复制所有源代码
COPY backend/ ./
# 构建和发布
WORKDIR /src/AmazonTrends.WebApp
RUN dotnet build -c Release --no-restore
RUN dotnet publish -c Release -o /app/publish --no-restore --no-build
# 运行时阶段
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AmazonTrends.WebApp.dll"]
