# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy tất cả file .csproj để restore nhanh hơn (giữ nguyên)
COPY services/Tasco.TaskService/Tasco.TaskService.API/Tasco.TaskService.API.csproj services/Tasco.TaskService/Tasco.TaskService.API/
COPY services/Tasco.TaskService/Tasco.TaskService.Repository/Tasco.TaskService.Repository.csproj services/Tasco.TaskService/Tasco.TaskService.Repository/
COPY services/Tasco.TaskService/Tasco.TaskService.Service/Tasco.TaskService.Service.csproj services/Tasco.TaskService/Tasco.TaskService.Service/
COPY services/Tasco.Shared/Tasco.Shared.Notifications/Tasco.Shared.Notifications.csproj services/Tasco.Shared/Tasco.Shared.Notifications/

# Restore
RUN dotnet restore services/Tasco.TaskService/Tasco.TaskService.API/Tasco.TaskService.API.csproj

# **Copy toàn bộ source code của services (bao gồm cả Tasco.Shared, Proto, ...)**
COPY services/ ./services/

# Build & publish
RUN dotnet publish services/Tasco.TaskService/Tasco.TaskService.API/Tasco.TaskService.API.csproj -c Release -o /app/publish

# Debug stage for troubleshooting
FROM build AS debug
RUN ls -lR /src/services/Tasco.Shared/Tasco.Shared.Notifications/
RUN ls -lR /src/services/Tasco.TaskService/Tasco.TaskService.API/Protos/

# Final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Tasco.TaskService.API.dll"]
