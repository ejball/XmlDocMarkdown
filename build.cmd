@echo off
pushd %~dp0
dotnet publish tools\Build\Build.csproj --output tools\bin\Build --nologo --verbosity quiet
dotnet tools\bin\Build\Build.dll %*
popd
