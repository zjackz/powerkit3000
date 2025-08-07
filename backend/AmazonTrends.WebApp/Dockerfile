# 使用官方的 .NET SDK 镜像作为构建阶段的基础镜像
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制项目文件并恢复 NuGet 包
# 复制 .sln 文件
COPY AmazonTrendsDashboard.sln .
# 复制所有项目文件，以便 dotnet restore 可以找到所有依赖
COPY backend/AmazonTrends.Core/AmazonTrends.Core.csproj backend/AmazonTrends.Core/
COPY backend/AmazonTrends.Core.Tests/AmazonTrends.Core.Tests.csproj backend/AmazonTrends.Core.Tests/
COPY backend/AmazonTrends.Data/AmazonTrends.Data.csproj backend/AmazonTrends.Data/
COPY backend/AmazonTrends.WebApp/AmazonTrends.WebApp.csproj backend/AmazonTrends.WebApp/

# 恢复所有项目的依赖
RUN dotnet restore "AmazonTrendsDashboard.sln"

# 复制所有源代码
COPY . .

# 发布后端应用
WORKDIR /src/backend/AmazonTrends.WebApp
RUN dotnet publish "AmazonTrends.WebApp.csproj" -c Release -o /app/publish --no-restore

# 使用官方的 .NET ASP.NET 运行时镜像作为最终镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# 复制发布的应用到最终镜像
COPY --from=build /app/publish .

# 暴露应用程序端口
EXPOSE 8080
EXPOSE 8081

# 定义应用程序的入口点
ENTRYPOINT ["dotnet", "AmazonTrends.WebApp.dll"]
