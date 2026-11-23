# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1

COPY CAPNEXAAPI.csproj ./
RUN dotnet restore CAPNEXAAPI.csproj

COPY . .
RUN dotnet publish CAPNEXAAPI.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM build AS migrate
ENV PATH="/root/.dotnet/tools:${PATH}"

RUN dotnet tool install --global dotnet-ef --version 10.0.12

ENTRYPOINT ["dotnet", "ef", "database", "update", \
    "--project", "CAPNEXAAPI.csproj", \
    "--startup-project", "CAPNEXAAPI.csproj", \
    "--configuration", "Release", \
    "--no-build"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1

RUN groupadd --system appuser \
    && useradd --system --gid appuser --home-dir /app --shell /usr/sbin/nologin appuser

COPY --from=build /app/publish ./

EXPOSE 8080
USER appuser

ENTRYPOINT ["dotnet", "CAPNEXAAPI.dll"]
