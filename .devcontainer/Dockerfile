# Use the official .NET 9 Dev Container image
FROM mcr.microsoft.com/devcontainers/dotnet:dev-9.0-bookworm-slim

# Install additional dependencies
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Create docker group and set socket permissions
RUN groupadd -g 999 docker || true \
    && usermod -aG docker $(whoami) \
    && chown root:docker /var/run/docker.sock || true \
    && sudo chmod 666 /var/run/docker.sock || true

# Install .NET Aspire workload
RUN dotnet workload install aspire

# Trust the ASP.NET Core HTTPS development certificate
RUN dotnet dev-certs https --trust

# Set working directory
WORKDIR /workspaces