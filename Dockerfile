# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ComicEncyclopedia.sln ./
COPY src/ComicEncyclopedia.Common/ComicEncyclopedia.Common.csproj src/ComicEncyclopedia.Common/
COPY src/ComicEncyclopedia.Data/ComicEncyclopedia.Data.csproj src/ComicEncyclopedia.Data/
COPY src/ComicEncyclopedia.Business/ComicEncyclopedia.Business.csproj src/ComicEncyclopedia.Business/
COPY src/ComicEncyclopedia.Web/ComicEncyclopedia.Web.csproj src/ComicEncyclopedia.Web/
COPY tests/ComicEncyclopedia.Tests/ComicEncyclopedia.Tests.csproj tests/ComicEncyclopedia.Tests/

RUN dotnet restore ComicEncyclopedia.sln

COPY . .

RUN dotnet build src/ComicEncyclopedia.Web/ComicEncyclopedia.Web.csproj \
    -c $BUILD_CONFIGURATION --no-restore -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish src/ComicEncyclopedia.Web/ComicEncyclopedia.Web.csproj \
    -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=0

EXPOSE 8080

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ComicEncyclopedia.Web.dll"]
