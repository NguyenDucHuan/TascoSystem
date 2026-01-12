FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first
COPY ["services/Tasco.UserAuthService/Tasco.UserAuthService.API/Tasco.UserAuthService.API.csproj", "Tasco.UserAuthService.API/"]
COPY ["services/Tasco.UserAuthService/Tasco.UserAuthService.Service/Tasco.UserAuthService.Service.csproj", "Tasco.UserAuthService.Service/"]
COPY ["services/Tasco.UserAuthService/Tasco.UserAuthService.Repository/Tasco.UserAuthService.Repository.csproj", "Tasco.UserAuthService.Repository/"]

# Restore packages
RUN dotnet restore "Tasco.UserAuthService.API/Tasco.UserAuthService.API.csproj"

# Copy source code
COPY services/Tasco.UserAuthService .

# Copy .env file
COPY services/Tasco.UserAuthService/Tasco.UserAuthService.API/.env .env

# Build and publish directly
RUN dotnet publish "Tasco.UserAuthService.API/Tasco.UserAuthService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Tasco.UserAuthService.API.dll"]