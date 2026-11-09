# syntax=docker/dockerfile:1

# ---------- stage 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Копируем только файлы проектов, чтобы слой restore кэшировался
# и не пересобирался при изменении исходного кода.
COPY PromoCodeFactory.slnx ./
COPY PromoCodeFactory.Core/PromoCodeFactory.Core.csproj PromoCodeFactory.Core/
COPY PromoCodeFactory.DataAccess/PromoCodeFactory.DataAccess.csproj PromoCodeFactory.DataAccess/
COPY PromoCodeFactory.WebHost/PromoCodeFactory.WebHost.csproj PromoCodeFactory.WebHost/

RUN dotnet restore ./PromoCodeFactory.WebHost/PromoCodeFactory.WebHost.csproj

# Копируем остальные файлы и собираем приложение.
COPY . .

RUN dotnet build ./PromoCodeFactory.WebHost/PromoCodeFactory.WebHost.csproj \
        -c $BUILD_CONFIGURATION --no-restore \
 && dotnet publish ./PromoCodeFactory.WebHost/PromoCodeFactory.WebHost.csproj \
        -c $BUILD_CONFIGURATION -o /app/publish --no-build --no-restore /p:UseAppHost=false

# ---------- stage 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# dll-ка, собранная на стадии build
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "PromoCodeFactory.WebHost.dll"]
