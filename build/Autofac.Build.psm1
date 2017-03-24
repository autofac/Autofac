# EXIT CODES
# 1: dotnet packaging failure
# 2: dotnet publishing failure
# 3: Unit test failure
# 4: dotnet / NuGet package restore failure

<#
 .SYNOPSIS
  Writes a build progress message to the host.

 .PARAMETER Message
  The message to write.
#>
function Write-Message
{
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True, ValueFromPipeline=$False, ValueFromPipelineByPropertyName=$False)]
    [ValidateNotNullOrEmpty()]
    [string]
    $Message
  )

  Write-Host "[BUILD] $Message" -ForegroundColor Cyan
}

<#
 .SYNOPSIS
  Gets the set of directories in which projects are available for compile/processing.

 .PARAMETER RootPath
  Path where searching for project directories should begin.
#>
function Get-DotNetProjectDirectory
{
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True, ValueFromPipeline=$False, ValueFromPipelineByPropertyName=$False)]
    [ValidateNotNullOrEmpty()]
    [string]
    $RootPath
  )

  Get-ChildItem -Path $RootPath -Recurse -Include "*.csproj" | Select-Object @{ Name="ParentFolder"; Expression={ $_.Directory.FullName.TrimEnd("\") } } | Select-Object -ExpandProperty ParentFolder
}

<#
 .SYNOPSIS
  Runs the dotnet CLI install script from GitHub to install a project-local
  copy of the CLI.
#>
function Install-DotNetCli
{
  [CmdletBinding()]
  Param(
    [string]
    $Version = "Latest"
  )

  if ((Get-Command "dotnet.exe" -ErrorAction SilentlyContinue) -ne $null)
  {
    $installedVersion = dotnet --version
    if ($installedVersion -eq $Version)
    {
      Write-Message ".NET Core SDK version $Version is already installed"
      return;
    }
  }

  $callerPath = Split-Path $MyInvocation.PSCommandPath
  $installDir = Join-Path -Path $callerPath -ChildPath ".dotnet\cli"
  if (!(Test-Path $installDir))
  {
    New-Item -ItemType Directory -Path "$installDir" | Out-Null
  }

  # Download the dotnet CLI install script
  if (!(Test-Path .\dotnet\install.ps1))
  {
    Invoke-WebRequest "https://raw.githubusercontent.com/dotnet/cli/rel/1.0.1/scripts/obtain/dotnet-install.ps1" -OutFile ".\.dotnet\dotnet-install.ps1"
  }

  # Run the dotnet CLI install
  & .\.dotnet\dotnet-install.ps1 -InstallDir "$installDir" -Version $Version

  # Add the dotnet folder path to the process.
  $env:PATH = "$installDir;$env:PATH"
}

<#
.SYNOPSIS
    Builds a project using dotnet cli.
.DESCRIPTION
    Builds a project in a specified directory using the dotnet cli.
.PARAMETER DirectoryName
    The path to the directory containing the project to build.
#>
function Invoke-DotNetBuild
{
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True, ValueFromPipeline=$True, ValueFromPipelineByPropertyName=$True)]
    [ValidateNotNull()]
    [System.IO.DirectoryInfo[]]
    $ProjectDirectory
  )
  Process
  {
    foreach($Project in $ProjectDirectory)
    {
      & dotnet build ("""" + $Project.FullName + """") --configuration Release
      if ($LASTEXITCODE -ne 0)
      {
        exit 1
      }
    }
  }
}

<#
 .SYNOPSIS
  Invokes the dotnet utility to package a project.

 .PARAMETER ProjectDirectory
  Path to the directory containing the project to package.

 .PARAMETER PackagesPath
  Path to the "artifacts\packages" folder where packages should go.

 .PARAMETER VersionSuffix
  The version suffix to use for the NuGet package version.
#>
function Invoke-DotNetPack
{
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True, ValueFromPipeline=$True, ValueFromPipelineByPropertyName=$True)]
    [ValidateNotNull()]
    [System.IO.DirectoryInfo[]]
    $ProjectDirectory,

    [Parameter(Mandatory=$True, ValueFromPipeline=$False)]
    [ValidateNotNull()]
    [System.IO.DirectoryInfo]
    $PackagesPath,

    [Parameter(Mandatory=$True, ValueFromPipeline=$False)]
    [ValidateNotNull()]
    [System.IO.DirectoryInfo]
    $VersionSuffix
  )
  Begin
  {
    New-Item -Path $PackagesPath -ItemType Directory -Force | Out-Null
  }
  Process
  {
    foreach($Project in $ProjectDirectory)
    {
      & dotnet build ("""" + $Project.FullName + """") --configuration Release --version-suffix $VersionSuffix
      & dotnet pack ("""" + $Project.FullName + """") --configuration Release --version-suffix $VersionSuffix --include-symbols --output $PackagesPath
      if ($LASTEXITCODE -ne 0)
      {
        exit 1
      }
    }
  }
}

<#
 .Synopsis
  Invokes dotnet test command.

 .Parameter ProjectDirectory
  Path to the directory containing the project to package.
#>
function Invoke-Test
{
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True, ValueFromPipeline=$True, ValueFromPipelineByPropertyName=$True)]
    [ValidateNotNull()]
    [System.IO.DirectoryInfo[]]
    $ProjectDirectory
  )
  Process
  {
    foreach($Project in $ProjectDirectory)
    {
      Push-Location $Project

      & dotnet test --configuration Release --logger:trx
      if ($LASTEXITCODE -ne 0)
      {
        Pop-Location
        exit 3
      }

      Pop-Location
    }
  }
}

<#
 .SYNOPSIS
  Restores dependencies using the dotnet utility.

 .PARAMETER ProjectDirectory
  Path to the directory containing the project with dependencies to restore.
#>
function Restore-DependencyPackages
{
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True, ValueFromPipeline=$True, ValueFromPipelineByPropertyName=$True)]
    [ValidateNotNull()]
    [System.IO.DirectoryInfo[]]
    $ProjectDirectory
  )
  Process
  {
    foreach($Project in $ProjectDirectory)
    {
      & dotnet restore ("""" + $Project.FullName + """") --no-cache
      if($LASTEXITCODE -ne 0)
      {
        exit 4
      }
    }
  }
}
