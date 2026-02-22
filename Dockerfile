# syntax=docker/dockerfile:1

# ------------------------------------------------------------------
# Stage 1: Build
# ------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY CognitiveMesh.sln Directory.Build.props ./
COPY src/FoundationLayer/*.csproj src/FoundationLayer/
COPY src/ReasoningLayer/*.csproj src/ReasoningLayer/
COPY src/MetacognitiveLayer/*.csproj src/MetacognitiveLayer/
COPY src/AgencyLayer/*.csproj src/AgencyLayer/
COPY src/BusinessApplications/*.csproj src/BusinessApplications/
COPY src/MeshSimRuntime/*.csproj src/MeshSimRuntime/
COPY src/Shared/*.csproj src/Shared/
COPY src/UILayer/*.csproj src/UILayer/
COPY tests/ tests/

# Restore NuGet packages
RUN dotnet restore CognitiveMesh.sln

# Copy remaining source
COPY . .

# Build in Release mode
RUN dotnet build CognitiveMesh.sln -c Release --no-restore

# Publish the runtime project (configurable via build arg)
ARG PUBLISH_PROJECT=src/MeshSimRuntime/MeshSimRuntime.csproj
RUN dotnet publish "${PUBLISH_PROJECT}" -c Release --no-build -o /app/publish

# ------------------------------------------------------------------
# Stage 2: Runtime
# ------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser

# Copy published output
COPY --from=build /app/publish .

# Configurable entrypoint DLL name
ENV ENTRYPOINT_DLL="MeshSimRuntime.dll"

# Expose default ASP.NET Core port
EXPOSE 8080

# Switch to non-root user
USER appuser

ENTRYPOINT ["sh", "-c", "dotnet ${ENTRYPOINT_DLL}"]
