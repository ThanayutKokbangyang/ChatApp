# =====================================================================
# Chat Service Dockerfile (โครงสร้างแบบแบน)
# วางที่: D:\ChatApp\ChatApp.ChatService.API\Dockerfile
# build context = D:\ChatApp (root)
# =====================================================================

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5001
ENV ASPNETCORE_URLS=http://+:5001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ChatApp.ChatService.API/ChatApp.ChatService.API.csproj", "ChatApp.ChatService.API/"]
COPY ["ChatApp.ChatService.Application/ChatApp.ChatService.Application.csproj", "ChatApp.ChatService.Application/"]
COPY ["ChatApp.ChatService.Domain/ChatApp.ChatService.Domain.csproj", "ChatApp.ChatService.Domain/"]
COPY ["ChatApp.ChatService.Infrastructure/ChatApp.ChatService.Infrastructure.csproj", "ChatApp.ChatService.Infrastructure/"]

RUN dotnet restore "ChatApp.ChatService.API/ChatApp.ChatService.API.csproj"

COPY ChatApp.ChatService.API/ ChatApp.ChatService.API/
COPY ChatApp.ChatService.Application/ ChatApp.ChatService.Application/
COPY ChatApp.ChatService.Domain/ ChatApp.ChatService.Domain/
COPY ChatApp.ChatService.Infrastructure/ ChatApp.ChatService.Infrastructure/

WORKDIR "/src/ChatApp.ChatService.API"
RUN dotnet publish "ChatApp.ChatService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
# สร้างโฟลเดอร์เก็บ avatar (mount เป็น volume)
RUN mkdir -p /app/wwwroot/uploads/avatars
ENTRYPOINT ["dotnet", "ChatApp.ChatService.API.dll"]
