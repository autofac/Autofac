########################
# THE BUILD!
########################

param (
    [switch]$Bench = $false
)

Push-Location $PSScriptRoot
try {
    Import-Module $PSScriptRoot/build/Autofac.Build.psd1 -Force

    $artifactsPath = "$PSScriptRoot/artifacts"
    $packagesPath = "$artifactsPath/packages"

    $globalJson = (Get-Content "$PSScriptRoot/global.json" | ConvertFrom-Json -NoEnumerate);

    $sdkVersion = $globalJson.sdk.version

    # Clean up artifacts folder
    if (Test-Path $artifactsPath) {
        Write-Message "Cleaning $artifactsPath folder"
        Remove-Item $artifactsPath -Force -Recurse
    }

    # Install dotnet SDK versions during CI. In a local build we assume you have
    # everything installed; on CI we'll force the install. If you install _any_
    # SDKs, you have to install _all_ of them because you can't install SDKs in
    # two different locations. dotnet CLI locates SDKs relative to the
    # executable.
    if ($Null -ne $env:APPVEYOR_BUILD_NUMBER) {
        Install-DotNetCli -Version $sdkVersion
        foreach ($additional in $globalJson.additionalSdks)
        {
            Install-DotNetCli -Version $additional;
        }
    }

    # Write out dotnet information
    & dotnet --info

    # Set version suffix
    $branch = @{ $true = $env:APPVEYOR_REPO_BRANCH; $false = $(git symbolic-ref --short -q HEAD) }[$NULL -ne $env:APPVEYOR_REPO_BRANCH];
    $revision = @{ $true = "{0:00000}" -f [convert]::ToInt32("0" + $env:APPVEYOR_BUILD_NUMBER, 10); $false = "local" }[$NULL -ne $env:APPVEYOR_BUILD_NUMBER];
    $versionSuffix = @{ $true = ""; $false = "$($branch.Substring(0, [math]::Min(10,$branch.Length)).Replace('/', '-'))-$revision" }[$branch -eq "master" -and $revision -ne "local"]

    Write-Message "Package version suffix is '$versionSuffix'"

    # Package restore
    Write-Message "Restoring packages"
    Get-DotNetProjectDirectory -RootPath $PSScriptRoot | Restore-DependencyPackages

    # Build/package
    Write-Message "Building projects and packages"
    Get-DotNetProjectDirectory -RootPath $PSScriptRoot\src | Invoke-DotNetPack -PackagesPath $packagesPath -VersionSuffix $versionSuffix

    # Test
    Write-Message "Executing unit tests"
    Get-DotNetProjectDirectory -RootPath $PSScriptRoot\test | Where-Object { $_ -inotlike "*Autofac.Test.Scenarios.ScannedAssembly" } | Invoke-Test

    # Benchmark
    if ($Bench) {
        Get-DotNetProjectDirectory -RootPath $PSScriptRoot\bench | Invoke-Test
        Get-ChildItem -Path $PSScriptRoot\bench -Filter "BenchmarkDotNet.Artifacts" -Directory -Recurse | Move-Item -Destination "$PSScriptRoot\artifacts\benchmarks"
    }

    if ($env:CI -eq "true") {
        # Generate Coverage Report
        Write-Message "Downloading and verifying Codecov Uploader"
        $ProgressPreference = 'SilentlyContinue'
        Invoke-WebRequest -Uri https://keybase.io/codecovsecurity/pgp_keys.asc -OutFile codecov.asc
        gpg --no-default-keyring --keyring trustedkeys.gpg --import codecov.asc
        Invoke-WebRequest -Uri https://uploader.codecov.io/latest/linux/codecov -Outfile codecov
        Invoke-WebRequest -Uri https://uploader.codecov.io/latest/linux/codecov.SHA256SUM -Outfile codecov.SHA256SUM
        Invoke-WebRequest -Uri https://uploader.codecov.io/latest/linux/codecov.SHA256SUM.sig -Outfile codecov.SHA256SUM.sig
        gpgv codecov.SHA256SUM.sig codecov.SHA256SUM
        shasum -a 256 -c codecov.SHA256SUM
        chmod +x codecov

        Write-Message "Generating Codecov Report"
        & ./codecov -f "artifacts/coverage/*/coverage*.info"
    }

    # Finished
    Write-Message "Build finished"
}
finally {
    Pop-Location
}
