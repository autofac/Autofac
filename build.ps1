# Build variables
$dnvmVersion = "1.0.0-beta5-11911";

########################
# FUNCTIONS
########################
function Install-Dnvm
{
    & where.exe dnvm 2>&1 | Out-Null
    if($LASTEXITCODE -ne 0)
    {
        Write-Host "DNVM not found"
        &{$Branch='dev';iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}

        # Normally this happens automatically during install but AppVeyor has
        # an issue where you may need to manually re-run setup from within this process.
        if($env:DNX_HOME -eq $NULL)
        {
            Write-Host "Initial DNVM environment setup failed; running manual setup"
            $tempDnvmPath = Join-Path $env:TEMP "dnvminstall"
            $dnvmSetupCmdPath = Join-Path $tempDnvmPath "dnvm.ps1"
            & $dnvmSetupCmdPath setup
        }
    }
}

function Restore-Packages
{
    param([string] $DirectoryName)
    & dnu restore ("""" + $DirectoryName + """")
}

function Build-Projects
{
    param([string] $DirectoryName)
    & dnu pack ("""" + $DirectoryName + """") --configuration Release --out .\artifacts\packages; if($LASTEXITCODE -ne 0) { exit 1 }
}

function Test-Projects
{
    param([string] $DirectoryName)
    & dnx ("""" + $DirectoryName + """") test; if($LASTEXITCODE -ne 0) { exit 2 }
}

########################
# THE BUILD!
########################

Push-Location $PSScriptRoot

# Clean
if(Test-Path .\artifacts) { Remove-Item .\artifacts -Force -Recurse }

# Install DNVM
Install-Dnvm

# Install DNX
dnvm install $dnvmVersion -r CoreCLR -u
dnvm install $dnvmVersion -r CLR -u
dnvm use $dnvmVersion -r CLR

# Package restore
Get-ChildItem -Path . -Filter *.xproj -Recurse | ForEach-Object { dnu restore ("""" + $_.DirectoryName + """") }

# Set build number
$env:DNX_BUILD_VERSION = @{ $true = $env:APPVEYOR_BUILD_NUMBER; $false = 1}[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
Write-Host "Build number:" $env:DNX_BUILD_VERSION

# Build/package
Get-ChildItem -Path .\src -Filter *.xproj -Recurse | ForEach-Object { Build-Projects $_.DirectoryName }

# Test
Get-ChildItem -Path .\test -Filter *.xproj -Exclude Autofac.Test.Scenarios.ScannedAssembly.xproj -Recurse | ForEach-Object { Test-Projects $_.DirectoryName }

Pop-Location
