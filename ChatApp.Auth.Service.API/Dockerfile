FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/Services/ChatApp.AuthService/ChatApp.AuthService.API/ChatApp.AuthService.API.csproj", "src/Services/ChatApp.AuthService/ChatApp.AuthService.API/"]
COPY ["src/Services/ChatApp.AuthService/ChatApp.AuthService.Application/ChatApp.AuthService.Application.csproj", "src/Services/ChatApp.AuthService/ChatApp.AuthService.Application/"]
COPY ["src/Services/ChatApp.AuthService/ChatApp.AuthService.Domain/ChatApp.AuthService.Domain.csproj", "src/Services/ChatApp.AuthService/ChatApp.AuthService.Domain/"]
COPY ["src/Services/ChatApp.AuthService/ChatApp.AuthService.Infrastructure/ChatApp.AuthService.Infrastructure.csproj", "src/Services/ChatApp.AuthService/ChatApp.AuthService.Infrastructure/"]

RUN dotnet restore "src/Services/ChatApp.AuthService/ChatApp.AuthService.API/ChatApp.AuthService.API.csproj"

COPY src/Services/ChatApp.AuthService/ src/Services/ChatApp.AuthService/

WORKDIR "/src/src/Services/ChatApp.AuthService/ChatApp.AuthService.API"
RUN dotnet publish "ChatApp.AuthService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ChatApp.AuthService.API.dll"]
