# syntax=docker/dockerfile:1
# Build context is the repository root:  docker build -f src/Base.Api/Dockerfile .

# ---- Build stage: full SDK, restores and publishes the API ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only project files first so the restore layer is cached until dependencies change.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Base.BuildingBlocks/Base.BuildingBlocks.csproj src/Base.BuildingBlocks/
COPY src/Modules/Todos/Base.Modules.Todos.csproj src/Modules/Todos/
COPY src/Base.Api/Base.Api.csproj src/Base.Api/
RUN dotnet restore src/Base.Api/Base.Api.csproj

COPY src/ src/
RUN dotnet publish src/Base.Api/Base.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---- Runtime stage: small "chiseled" image with no shell or package manager; runs as non-root ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Base.Api.dll"]
