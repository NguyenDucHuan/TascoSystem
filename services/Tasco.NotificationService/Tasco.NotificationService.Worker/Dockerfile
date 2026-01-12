FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["services/Tasco.NotificationService/Tasco.NotificationService.Worker/Tasco.NotificationService.Worker.csproj", "Tasco.NotificationService.Worker/"]
COPY ["services/Tasco.NotificationService/Tasco.NotificationService.Core/Tasco.NotificationService.Core.csproj", "Tasco.NotificationService.Core/"]
COPY ["services/Tasco.NotificationService/Tasco.NotificationService.Infrastructure/Tasco.NotificationService.Infrastructure.csproj", "Tasco.NotificationService.Infrastructure/"]

# Restore packages
RUN dotnet restore "./Tasco.NotificationService.Worker/Tasco.NotificationService.Worker.csproj"

# Copy source code
COPY services/Tasco.NotificationService .

WORKDIR "/src/Tasco.NotificationService.Worker"
RUN dotnet build "./Tasco.NotificationService.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tasco.NotificationService.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tasco.NotificationService.Worker.dll"]