# Use the official .NET 9 Dev Container image
FROM mcr.microsoft.com/devcontainers/dotnet:dev-9.0-bookworm-slim

# Install additional dependencies (including ca-certificates for trust)
RUN apt-get update && apt-get install -y \
    curl \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Install .NET Aspire workload
RUN dotnet workload install aspire

# Ensure the container user is in the docker group
RUN usermod -aG docker codespace || true

# Set working directory
WORKDIR /workspaces