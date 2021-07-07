#!/usr/bin/env pwsh
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
  if (-not (Test-Path ./tools/bin/Build) -or
      (Get-ChildItem ./tools/Build/* | Measure-Object LastWriteTime -Maximum).Maximum -gt
      (Get-ChildItem ./tools/bin/Build/* | Measure-Object LastWriteTime -Maximum).Maximum) {
    if (Test-Path ./tools/bin/Build) { Remove-Item ./tools/bin/Build -Recurse }
    dotnet publish ./tools/Build/Build.csproj --output ./tools/bin/Build --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  dotnet ./tools/bin/Build/Build.dll $args
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
finally {
  Pop-Location
}
