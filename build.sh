#!/usr/bin/env bash
set -euo pipefail
cd "$( dirname "${BASH_SOURCE[0]}" )"
dotnet run --project tools/Build/Build.csproj -- "$@"
