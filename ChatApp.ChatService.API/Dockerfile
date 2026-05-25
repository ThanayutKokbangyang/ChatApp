FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5001
ENV ASPNETCORE_URLS=http://+:5001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/Services/ChatApp.ChatService/ChatApp.ChatService.API/ChatApp.ChatService.API.csproj", "src/Services/ChatApp.ChatService/ChatApp.ChatService.API/"]
COPY ["src/Services/ChatApp.ChatService/ChatApp.ChatService.Application/ChatApp.ChatService.Application.csproj", "src/Services/ChatApp.ChatService/ChatApp.ChatService.Application/"]
COPY ["src/Services/ChatApp.ChatService/ChatApp.ChatService.Domain/ChatApp.ChatService.Domain.csproj", "src/Services/ChatApp.ChatService/ChatApp.ChatService.Domain/"]
COPY ["src/Services/ChatApp.ChatService/ChatApp.ChatService.Infrastructure/ChatApp.ChatService.Infrastructure.csproj", "src/Services/ChatApp.ChatService/ChatApp.ChatService.Infrastructure/"]\

RUN dotnet restore "src/Services/ChatApp.ChatService/ChatApp.ChatService.API/ChatApp.ChatService.API.csproj"

COPY src/Services/ChatApp.ChatService/ src/Services/ChatApp.ChatService/

WORKDIR "/src/src/Services/ChatApp.ChatService/ChatApp.ChatService.API"
RUN dotnet publish "ChatApp.ChatService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/wwwroot/uploads/avatars
ENTRYPOINT ["dotnet", "ChatApp.ChatService.API.dll"]