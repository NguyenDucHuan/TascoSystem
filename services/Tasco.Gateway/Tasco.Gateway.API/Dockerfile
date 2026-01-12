FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file first
COPY ["services/Tasco.Gateway/Tasco.Gateway.API/Tasco.Gateway.API.csproj", "Tasco.Gateway.API/"]

# Restore packages
RUN dotnet restore "Tasco.Gateway.API/Tasco.Gateway.API.csproj"

# Copy source code (without build artifacts)
COPY services/Tasco.Gateway .

# Build and publish directly (skip separate build step)
RUN dotnet publish "Tasco.Gateway.API/Tasco.Gateway.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Tasco.Gateway.API.dll"]