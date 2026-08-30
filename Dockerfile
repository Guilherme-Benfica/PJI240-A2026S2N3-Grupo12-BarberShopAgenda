# syntax=docker/dockerfile:1

# ---- Stage 1: build/publish ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia apenas os .csproj primeiro para aproveitar o cache de camadas do restore
COPY BarberShopAgenda.slnx ./
COPY BarberShopAgenda.API/BarberShopAgenda.API.csproj BarberShopAgenda.API/
COPY BarberShopAgenda.Domain/BarberShopAgenda.Domain.csproj BarberShopAgenda.Domain/
COPY BarberShopAgenda.Infrastructure/BarberShopAgenda.Infrastructure.csproj BarberShopAgenda.Infrastructure/

RUN dotnet restore BarberShopAgenda.API/BarberShopAgenda.API.csproj

# Copia o restante do código-fonte e publica
COPY BarberShopAgenda.API/ BarberShopAgenda.API/
COPY BarberShopAgenda.Domain/ BarberShopAgenda.Domain/
COPY BarberShopAgenda.Infrastructure/ BarberShopAgenda.Infrastructure/

RUN dotnet publish BarberShopAgenda.API/BarberShopAgenda.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ---- Stage 2: runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Imagem base do .NET 8 já traz o usuário não-root "app"
USER app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BarberShopAgenda.API.dll"]
