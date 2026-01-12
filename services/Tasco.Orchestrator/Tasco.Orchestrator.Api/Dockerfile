FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first
COPY ["services/Tasco.Orchestrator/Tasco.Orchestrator.Api/Tasco.Orchestrator.Api.csproj", "Tasco.Orchestrator.Api/"]
COPY ["services/Tasco.Orchestrator/Tasco.Orchestrator.Infrastructure/Tasco.Orchestrator.Infrastructure.csproj", "Tasco.Orchestrator.Infrastructure/"]
COPY ["services/Tasco.Orchestrator/Tasco.Orchestrator.Core/Tasco.Orchestrator.Core.csproj", "Tasco.Orchestrator.Core/"]

# Restore packages
RUN dotnet restore "Tasco.Orchestrator.Api/Tasco.Orchestrator.Api.csproj"

# Copy source code
COPY services/Tasco.Orchestrator .

# Build and publish directly
RUN dotnet publish "Tasco.Orchestrator.Api/Tasco.Orchestrator.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Tasco.Orchestrator.Api.dll"]