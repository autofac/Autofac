# EXIT CODES
# 1: dotnet packaging failure
# 2: dotnet publishing failure
# 3: Unit test failure
# 4: dotnet / NuGet package restore failure

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

  # We don't search for project.json because that gets copied around. .xproj is the only
  # good way to actually locate where the source project is.
  Get-ChildItem -Path $RootPath -Recurse -Include "*.xproj" | Select-Object @{ Name="ParentFolder"; Expression={ $_.Directory.FullName.TrimEnd("\") } } | Select-Object -ExpandProperty ParentFolder
}

<#
 .SYNOPSIS
  Gets the SDK version specified in a global.json, if any. Defaults to "Latest".

 .PARAMETER GlobalJson
  Path to the global.json file.
#>
function Get-DotNetSdkVersion
{
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True, ValueFromPipeline=$False, ValueFromPipelineByPropertyName=$False)]
    [ValidateNotNullOrEmpty()]
    [string]
    $GlobalJson
  )

  $version = (Get-Content $GlobalJson | ConvertFrom-Json).sdk.version

  if ($version -eq $Null)
  {
    return "Latest"
  }

  return $version
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

  $callerPath = Split-Path $MyInvocation.PSCommandPath
  $installDir = Join-Path -Path $callerPath -ChildPath ".dotnet\cli"
  if (!(Test-Path $installDir))
  {
    New-Item -ItemType Directory -Path "$installDir" | Out-Null
  }

  # Download the dotnet CLI install script
  if (!(Test-Path .\dotnet\install.ps1))
  {
    Invoke-WebRequest "https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain/dotnet-install.ps1" -OutFile ".\.dotnet\dotnet-install.ps1"
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
      if($LASTEXITCODE -ne 0)
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
    $PackagesPath
  )
  Begin
  {
    New-Item -Path $PackagesPath -ItemType Directory -Force | Out-Null
  }
  Process
  {
    foreach($Project in $ProjectDirectory)
    {
      & dotnet build ("""" + $Project.FullName + """") --configuration Release
      & dotnet pack ("""" + $Project.FullName + """") --configuration Release --output $PackagesPath
      if($LASTEXITCODE -ne 0)
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
      & dotnet test ("""" + $Project.FullName + """")
      if($LASTEXITCODE -ne 0)
      {
        exit 3
      }
    }
  }
}

<#
.SYNOPSIS
    Removes a path entry from the current user and process path.
.DESCRIPTION
    Updates the user and process paths as needed to remove a specific path
    from the overall search path.
.PARAMETER VariableToRemove
    The directory/path that should be removed.
#>
function Remove-EnvironmentPathEntry
{
  [cmdletbinding()]
  param([string] $VariableToRemove)
  $path = [Environment]::GetEnvironmentVariable("PATH", "User")
  $newItems = $path.Split(';') | Where-Object { $_.ToString() -inotlike $VariableToRemove }
  [Environment]::SetEnvironmentVariable("PATH", [System.String]::Join(';', $newItems), "User")
  $path = [Environment]::GetEnvironmentVariable("PATH", "Process")
  $newItems = $path.Split(';') | Where-Object { $_.ToString() -inotlike $VariableToRemove }
  [Environment]::SetEnvironmentVariable("PATH", [System.String]::Join(';', $newItems), "Process")
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
