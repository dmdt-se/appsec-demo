#!/bin/bash
set -e

echo "=== AppSec .NET Demo Publish Script ==="
echo ""

# Clean previous builds
echo "[1/5] Cleaning previous builds..."
rm -rf publish/
rm -rf src/AppSecDotnetDemo/bin/
rm -rf src/AppSecDotnetDemo/obj/

# Build plugins
echo "[2/5] Building plugins..."
dotnet build plugins/LegitimatePlugin -c Release
dotnet build plugins/HardcodedAttackPlugin -c Release
dotnet build plugins/TaintedInputPlugin -c Release

# Publish main application
echo "[3/5] Publishing application for Linux x64..."
dotnet publish src/AppSecDotnetDemo -c Release -r linux-x64 --self-contained false -o ./publish

# Copy plugins
echo "[4/5] Copying plugins to publish directory..."
mkdir -p publish/assemblies
cp plugins/LegitimatePlugin/bin/Release/net8.0/LegitimatePlugin.dll publish/assemblies/
cp plugins/HardcodedAttackPlugin/bin/Release/net8.0/HardcodedAttackPlugin.dll publish/assemblies/
cp plugins/TaintedInputPlugin/bin/Release/net8.0/TaintedInputPlugin.dll publish/assemblies/

# Create archive
echo "[5/5] Creating deployment archive..."
tar -czvf appsec-dotnet-demo.tar.gz -C publish .

echo ""
echo "=== Publish Complete ==="
echo "Artifacts:"
echo "  - publish/                 (deployment directory)"
echo "  - appsec-dotnet-demo.tar.gz    (deployment archive)"
echo ""
echo "To deploy to server:"
echo "  scp appsec-dotnet-demo.tar.gz user@server:/opt/appsec-dotnet-demo/"
echo "  ssh user@server 'cd /opt/appsec-dotnet-demo && tar -xzvf appsec-dotnet-demo.tar.gz'"
