# =====================================================================
# Auth Service Dockerfile (โครงสร้างแบบแบน)
# วางที่: D:\ChatApp\ChatApp.AuthService.API\Dockerfile
# build context = D:\ChatApp (root) ตาม docker-compose.yml
# =====================================================================

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# COPY .csproj ก่อน เพื่อใช้ layer cache ของ restore
COPY ["ChatApp.AuthService.API/ChatApp.AuthService.API.csproj", "ChatApp.AuthService.API/"]
COPY ["ChatApp.AuthService.Application/ChatApp.AuthService.Application.csproj", "ChatApp.AuthService.Application/"]
COPY ["ChatApp.AuthService.Domain/ChatApp.AuthService.Domain.csproj", "ChatApp.AuthService.Domain/"]
COPY ["ChatApp.AuthService.Infrastructure/ChatApp.AuthService.Infrastructure.csproj", "ChatApp.AuthService.Infrastructure/"]

RUN dotnet restore "ChatApp.AuthService.API/ChatApp.AuthService.API.csproj"

# COPY source ทั้งหมดของ project ที่เกี่ยวข้อง
COPY ChatApp.AuthService.API/ ChatApp.AuthService.API/
COPY ChatApp.AuthService.Application/ ChatApp.AuthService.Application/
COPY ChatApp.AuthService.Domain/ ChatApp.AuthService.Domain/
COPY ChatApp.AuthService.Infrastructure/ ChatApp.AuthService.Infrastructure/

WORKDIR "/src/ChatApp.AuthService.API"
RUN dotnet publish "ChatApp.AuthService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ChatApp.AuthService.API.dll"]
