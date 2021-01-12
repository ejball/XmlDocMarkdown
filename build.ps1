Push-Location $PSScriptRoot
try {
  if (-not (Test-Path ./tools/bin/Build) -or
      (Get-ChildItem ./tools/Build/* | Measure-Object LastWriteTime -Maximum).Maximum -gt
      (Get-ChildItem ./tools/bin/Build/* | Measure-Object LastWriteTime -Maximum).Maximum) {
    dotnet publish ./tools/Build/Build.csproj --output ./tools/bin/Build --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw }
  }
  dotnet ./tools/bin/Build/Build.dll $args
}
finally {
  Pop-Location
  exit $LASTEXITCODE
}
