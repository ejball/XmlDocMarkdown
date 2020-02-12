@echo off
pushd %~dp0
dotnet run --project tools\Build\Build.csproj -- %*
popd
