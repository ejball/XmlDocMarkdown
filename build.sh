#!/usr/bin/env bash
set -euo pipefail
cd "$( dirname "${BASH_SOURCE[0]}" )"
dotnet publish tools/Build/Build.csproj --output tools/bin/Build
dotnet tools/bin/Build/Build.dll "$@"
