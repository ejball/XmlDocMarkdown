Push-Location $PSScriptRoot
dotnet publish ./tools/Build/Build.csproj --output ./tools/bin/Build --nologo --verbosity quiet
if ($LASTEXITCODE -eq 0) { dotnet ./tools/bin/Build/Build.dll $args }
Pop-Location
exit $LASTEXITCODE
