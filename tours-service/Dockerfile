# ===== Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# ===== Build/Restore =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1) Kopiraj SAMO .csproj (radi keširanja i da restore "vidi" oba projekta)
COPY shared/logging/Logging.csproj shared/logging/
COPY tours-service/ToursService/ToursService.csproj tours-service/ToursService/

# (Ako koristiš Directory.Packages.props ili NuGet.config, kopiraj i njih)
# COPY Directory.Packages.props ./
# COPY NuGet.config ./

# 2) Restore
RUN dotnet restore tours-service/ToursService/ToursService.csproj

# 3) Tek sada kopiraj OSTATak koda
COPY . .

# 4) Build/Publish
WORKDIR /src/tours-service/ToursService
RUN dotnet build ToursService.csproj -c Release -o /app/build
RUN dotnet publish ToursService.csproj -c Release -o /app/publish

# ===== Final =====
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Docker
EXPOSE 80
CMD ["dotnet", "ToursService.dll"]

# ===== Migrations (opciono) =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS migration-base
WORKDIR /src
COPY . .
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet tool install --global dotnet-ef --version 9.*
WORKDIR /src/tours-service/ToursService
CMD ["dotnet-ef", "database", "update"]
