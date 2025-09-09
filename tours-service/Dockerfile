# Bazni runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .

WORKDIR /src/ToursService
RUN dotnet restore ToursService.csproj
RUN dotnet build ToursService.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish ToursService.csproj -c Release -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configure ASP.NET Core to listen on port 80 inside the container
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENV ASPNETCORE_ENVIRONMENT=Docker

CMD ["dotnet", "ToursService.dll"]

# ========== MIGRATION STAGE ==========

FROM build AS migration-base
WORKDIR /src/ToursService
ENV PATH="$PATH:/root/.dotnet/tools"

# Install EF CLI
RUN dotnet tool install --global dotnet-ef --version 9.*
FROM migration-base AS execute-migration



# Default command for migration stage
CMD ["dotnet-ef", "database", "update"]

