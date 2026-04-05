# Multi-stage build for .NET 10 backend (linux-arm64)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /src

# Copy solution and projects
COPY ["HomeAssistant.sln", "."]
COPY ["Directory.Build.props", "Directory.Packages.props", "."]
COPY ["HomeAssistant.Domain/", "HomeAssistant.Domain/"]
COPY ["HomeAssistant.Application/", "HomeAssistant.Application/"]
COPY ["HomeAssistant.Infrastructure.Persistence/", "HomeAssistant.Infrastructure.Persistence/"]
COPY ["HomeAssistant.Infrastructure.Sensors/", "HomeAssistant.Infrastructure.Sensors/"]
COPY ["HomeAssistant.Infrastructure.Messaging/", "HomeAssistant.Infrastructure.Messaging/"]
COPY ["HomeAssistant.Infrastructure.HomeAssistant/", "HomeAssistant.Infrastructure.HomeAssistant/"]
COPY ["HomeAssistant.Integrations.OpenMeteo/", "HomeAssistant.Integrations.OpenMeteo/"]
COPY ["HomeAssistant.Presentation/", "HomeAssistant.Presentation/"]

# Restore and publish self-contained for linux-arm64
RUN dotnet restore
RUN dotnet publish -c Release -r linux-arm64 --self-contained true \
    -p:PublishTrimmed=true \
    -p:PublishReadyToRun=true \
    -o /app/publish \
    HomeAssistant.Presentation/HomeAssistant.Presentation.csproj

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-arm64v8
WORKDIR /app

# Install ca-certificates for HTTPS
RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*

COPY --from=builder /app/publish .

EXPOSE 5064
ENV ASPNETCORE_HTTP_PORTS=5064
ENTRYPOINT ["./HomeAssistant.Presentation"]

