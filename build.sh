#!/usr/bin/env bash
set -euo pipefail
export MONO_ROOT=$(dirname $(which mono))/../
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
dotnet run --project "$SCRIPT_DIR/tools/Build/Build.csproj" -- "$@"
