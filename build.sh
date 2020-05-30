#!/usr/bin/env bash
set -euo pipefail
export MONO_ROOT=$(dirname $(which mono))/../
cd "$( dirname "${BASH_SOURCE[0]}" )"
dotnet publish ./tools/Build/Build.csproj --output ./tools/bin/Build --nologo --verbosity quiet
dotnet ./tools/bin/Build/Build.dll "$@"
