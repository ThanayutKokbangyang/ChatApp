# =====================================================================
# Blazor Web Dockerfile (โครงสร้างแบบแบน)
# วางที่: D:\ChatApp\ChatApp.Web\Dockerfile
# build context = D:\ChatApp (root)
# ---------------------------------------------------------------------
# Blazor WASM = static files → serve ด้วย nginx เล็กๆ ใน container นี้
# (คนละตัวกับ nginx gateway ข้างนอก)
# =====================================================================

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ChatApp.Web/ChatApp.Web.csproj", "ChatApp.Web/"]
RUN dotnet restore "ChatApp.Web/ChatApp.Web.csproj"

COPY ChatApp.Web/ ChatApp.Web/
WORKDIR "/src/ChatApp.Web"
RUN dotnet publish "ChatApp.Web.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
# blazor.conf อยู่ที่ root\nginx\blazor.conf (path แบบแบน)
COPY nginx/blazor.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
