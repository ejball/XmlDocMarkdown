<#
.SYNOPSIS
This is a Powershell script to bootstrap a Cake build.
#>

[CmdletBinding()]
Param(
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$ScriptArgs
)

# create cake directory
$CakeDirPath = Join-Path $PSScriptRoot "cake"
New-Item -Path $CakeDirPath -Type Directory -ErrorAction SilentlyContinue | Out-Null

# create packages.config
$PackagesConfigPath = Join-Path $CakeDirPath "packages.config"
If (!(Test-Path $PackagesConfigPath)) {
    [System.IO.File]::WriteAllLines($PackagesConfigPath, @(
        "<?xml version=`"1.0`" encoding=`"utf-8`"?>",
        "<packages>",
        "`t<package id=`"Cake`" version=`"0.17.0`" />",
        "</packages>"))
}

# download nuget.exe if not in path and not already downloaded
$NuGetExe = Get-Command "nuget.exe" -ErrorAction SilentlyContinue
If ($NuGetExe -ne $null) {
    $NuGetExePath = $NuGetExe.Path
}
Else {
    $NuGetExePath = Join-Path $CakeDirPath "nuget.exe"
    If (!(Test-Path $NuGetExePath)) {
        Invoke-WebRequest -Uri http://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile $NuGetExePath
    }
}

# use NuGet to download Cake
Push-Location $CakeDirPath
Invoke-Expression "&`"$NuGetExePath`" install -ExcludeVersion -OutputDirectory ."
If ($LASTEXITCODE -ne 0) {
    Throw "An error occured while restoring NuGet tools."
}
Pop-Location

# run Cake with specified arguments
$CakeExePath = Join-Path $CakeDirPath "Cake/Cake.exe"
Invoke-Expression "& `"$CakeExePath`" --paths_tools=cake -experimental $ScriptArgs"
Exit $LASTEXITCODE
