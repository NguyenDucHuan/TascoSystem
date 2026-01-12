# Dockerfile cho Tasco.ProjectService

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy tất cả file .csproj để restore nhanh hơn (giữ nguyên)
COPY services/Tasco.ProjectService/Tasco.ProjectService.API/Tasco.ProjectService.API.csproj Tasco.ProjectService.API/
COPY services/Tasco.ProjectService/Tasco.ProjectService.Repository/Tasco.ProjectService.Repository.csproj Tasco.ProjectService.Repository/
COPY services/Tasco.ProjectService/Tasco.ProjectService.Service/Tasco.ProjectService.Service.csproj Tasco.ProjectService.Service/
COPY services/Tasco.Shared/Tasco.Shared.Notifications/Tasco.Shared.Notifications.csproj Tasco.Shared.Notifications/

# Restore
RUN dotnet restore Tasco.ProjectService.API/Tasco.ProjectService.API.csproj

# **Copy toàn bộ source code của services (bao gồm cả Tasco.Shared, Proto, ...)**
COPY services/ ./services/

# Build & publish
RUN dotnet publish services/Tasco.ProjectService/Tasco.ProjectService.API/Tasco.ProjectService.API.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Tasco.ProjectService.API.dll"]