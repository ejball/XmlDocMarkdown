#!/usr/bin/env bash
set -euo pipefail
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
export MONO_ROOT=$(dirname $(which mono))/../
echo $MONO_ROOT
dotnet run --project "$SCRIPT_DIR/tools/Build/Build.csproj" -- "$@"
