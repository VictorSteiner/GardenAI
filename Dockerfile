# Multi-stage build for .NET 10 backend (linux-arm64)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /src

# Copy solution and projects
COPY ["GardenAI.sln", "."]
COPY ["Directory.Build.props", "Directory.Packages.props", "."]
COPY ["GardenAI.Domain/", "GardenAI.Domain/"]
COPY ["GardenAI.Application/", "GardenAI.Application/"]
COPY ["GardenAI.Infrastructure.Persistence/", "GardenAI.Infrastructure.Persistence/"]
COPY ["GardenAI.Infrastructure.Sensors/", "GardenAI.Infrastructure.Sensors/"]
COPY ["GardenAI.Infrastructure.Messaging/", "GardenAI.Infrastructure.Messaging/"]
COPY ["GardenAI.Infrastructure.GardenAI/", "GardenAI.Infrastructure.GardenAI/"]
COPY ["GardenAI.Integrations.OpenMeteo/", "GardenAI.Integrations.OpenMeteo/"]
COPY ["GardenAI.Presentation/", "GardenAI.Presentation/"]

# Restore and publish self-contained for linux-arm64
RUN dotnet restore
RUN dotnet publish -c Release -r linux-arm64 --self-contained true \
    -p:PublishTrimmed=true \
    -p:PublishReadyToRun=true \
    -o /app/publish \
    GardenAI.Presentation/GardenAI.Presentation.csproj

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-arm64v8
WORKDIR /app

# Install ca-certificates for HTTPS
RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*

COPY --from=builder /app/publish .

EXPOSE 5064
ENV ASPNETCORE_HTTP_PORTS=5064
ENTRYPOINT ["./GardenAI.Presentation"]

