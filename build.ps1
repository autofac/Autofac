########################
# THE BUILD!
########################
Push-Location $PSScriptRoot
Import-Module $PSScriptRoot\Build\Autofac.Build.psd1 -Force

$env:DOTNET_INSTALL_DIR="$(Convert-Path "$PSScriptRoot")\.dotnet\win7-x64"
if (!(Test-Path $env:DOTNET_INSTALL_DIR))
{
  mkdir $env:DOTNET_INSTALL_DIR | Out-Null
}

Install-DotNetCli

# Add the dotnet folder path to the process. This gets skipped
# by Install-DotNetCli if it's already installed.
Remove-PathVariable $env:DOTNET_INSTALL_DIR
$env:PATH = "$env:DOTNET_INSTALL_DIR;$env:PATH"

# Set build number
$env:DOTNET_BUILD_VERSION = @{ $true = $env:APPVEYOR_BUILD_NUMBER; $false = 1}[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
Write-Host "Build number:" $env:DOTNET_BUILD_VERSION

# Clean
if(Test-Path .\artifacts) { Remove-Item .\artifacts -Force -Recurse }

# Package restore
& dotnet restore --infer-runtimes

# Build/package
Get-ChildItem -Path .\src -Filter *.xproj -Recurse | ForEach-Object { Invoke-DotNetPack $_.DirectoryName }
Get-ChildItem -Path .\samples -Filter *.xproj -Recurse | ForEach-Object { Invoke-DotNetBuild $_.DirectoryName }

# Publish tests so we can test without recompiling
Get-ChildItem -Path .\test -Filter *.xproj -Exclude Autofac.Test.Scenarios.ScannedAssembly.xproj -Recurse | ForEach-Object -Begin { $TestIndex = 0 } -Process { Publish-TestProject -DirectoryName $_.DirectoryName -Index $TestIndex; $TestIndex++; }

# Test under CLR
Invoke-Tests

# Switch to Core CLR
#dnvm use $netcoreVersion -r CoreCLR

# Test under Core CLR
#Invoke-Tests

Pop-Location
