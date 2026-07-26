FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY EvArkadasim.sln ./
COPY Application/Application.csproj Application/
COPY Core/Core.csproj Core/
COPY Domain/Domain.csproj Domain/
COPY Persistence/Persistence.csproj Persistence/
COPY EvArkadasim.API/EvArkadasim.API.csproj EvArkadasim.API/
RUN dotnet restore EvArkadasim.API/EvArkadasim.API.csproj

COPY . .
RUN dotnet publish EvArkadasim.API/EvArkadasim.API.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/wwwroot/uploads && chown -R app:app /app/wwwroot

USER app
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5118
HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=5 \
    CMD curl --fail --silent http://localhost:5118/health || exit 1

ENTRYPOINT ["dotnet", "EvArkadasim.API.dll"]
