#!/bin/bash
set -e

# Define the version from global.json or default to 9.0.104
DOTNET_VERSION="9.0.104"

echo "Installing .NET SDK version $DOTNET_VERSION..."

# Download and run the official install script
# We install to the default location: ~/.dotnet
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version "$DOTNET_VERSION"

# Configure environment variables for the current user
DOTNET_ROOT="$HOME/.dotnet"

echo "Configuring environment variables..."

# Add exports to .bashrc if not already present
if ! grep -q "export DOTNET_ROOT=" "$HOME/.bashrc"; then
    echo "export DOTNET_ROOT=$DOTNET_ROOT" >> "$HOME/.bashrc"
fi

# Add to PATH if not already present
# We check for the specific PATH export string
if ! grep -q 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' "$HOME/.bashrc"; then
    echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> "$HOME/.bashrc"
fi

echo ".NET SDK $DOTNET_VERSION installation complete."
