#!/usr/bin/env bash
set -euo pipefail
export MONO_ROOT=$(dirname $(which mono))/../
cd "$( dirname "${BASH_SOURCE[0]}" )"
dotnet run --project tools/Build/Build.csproj -- "$@"
