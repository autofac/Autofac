########################
# FUNCTIONS
########################
function Install-Dnvm
{
    & where.exe dnvm 2>&1 | Out-Null
    if(($LASTEXITCODE -ne 0) -Or ((Test-Path Env:\APPVEYOR) -eq $true))
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

function Get-DnxVersion
{
    $globalJson = join-path $PSScriptRoot "global.json"
    $jsonData = Get-Content -Path $globalJson -Raw | ConvertFrom-JSON
    return $jsonData.sdk.version
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

$dnxVersion = Get-DnxVersion

# Clean
if(Test-Path .\artifacts) { Remove-Item .\artifacts -Force -Recurse }

# Install DNVM
Install-Dnvm

# Install DNX
dnvm install $dnxVersion -r CoreCLR -Unstable -NoNative
dnvm install $dnxVersion -r CLR -Unstable -NoNative
dnvm use $dnxVersion -r CLR

# Package restore
Get-ChildItem -Path . -Filter *.xproj -Recurse | ForEach-Object { dnu restore ("""" + $_.DirectoryName + """") }

# Set build number
$env:DNX_BUILD_VERSION = @{ $true = $env:APPVEYOR_BUILD_NUMBER; $false = 1}[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
Write-Host "Build number:" $env:DNX_BUILD_VERSION

# Build/package
Get-ChildItem -Path .\src -Filter *.xproj -Recurse | ForEach-Object { Build-Projects $_.DirectoryName }

# Test
Get-ChildItem -Path .\test -Filter *.xproj -Exclude Autofac.Test.Scenarios.ScannedAssembly.xproj -Recurse | ForEach-Object { Test-Projects $_.DirectoryName }

# Switch to Core CLR
dnvm use $dnxVersion -r CoreCLR

# Test again
Get-ChildItem -Path .\test -Filter *.xproj -Exclude Autofac.Test.Scenarios.ScannedAssembly.xproj -Recurse | ForEach-Object { Test-Projects $_.DirectoryName }

Pop-Location
