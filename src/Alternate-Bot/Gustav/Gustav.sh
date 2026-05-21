#!/bin/sh
# NomadGustav.sh - Always cleans, rebuilds, and runs (dev mode)
rm -rf bin obj
dotnet build
dotnet run --no-build

# Uncomment for release mode (skip rebuild if already built)
# if [ -d "bin" ]; then
#   dotnet run --no-build
# else
#   dotnet build
#   dotnet run --no-build
# fi
