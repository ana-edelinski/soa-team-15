# Bazni runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

WORKDIR /src/StakeholdersService
RUN dotnet restore StakeholdersService.csproj
RUN dotnet build StakeholdersService.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish StakeholdersService.csproj -c Release -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configure ASP.NET Core to listen on port 80 inside the container
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENV ASPNETCORE_ENVIRONMENT=Docker

CMD ["dotnet", "StakeholdersService.dll"]

# ========== MIGRATION STAGE ==========

FROM build AS migration
WORKDIR /src/StakeholdersService

# Install EF CLI
RUN dotnet tool install --global dotnet-ef --version 8.*

# Make tools available
ENV PATH="$PATH:/root/.dotnet/tools"

# Default command for migration stage
CMD ["dotnet-ef", "database", "update"]

