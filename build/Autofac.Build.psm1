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
  [cmdletbinding()]
  param([string] $DirectoryName)
  & dotnet build ("""" + $DirectoryName + """") -c Release; if($LASTEXITCODE -ne 0) { exit 1 }
}

<#
.SYNOPSIS
    Invokes published test projects.
.DESCRIPTION
    Looks through the published test artifacts folder and executes all the test commands.
#>
function Invoke-Tests
{
  Get-ChildItem -Path .\test -Filter *.xproj -Recurse | ForEach-Object { & dotnet test $_.DirectoryName; if($LASTEXITCODE -ne 0) { exit 3 } }
}

<#
.SYNOPSIS
    Packages a project using dotnet cli.
.DESCRIPTION
    Builds and packages a project in a specified directory using the dotnet cli.
.PARAMETER DirectoryName
    The path to the directory containing the project to package.
#>
function Invoke-DotNetPack
{
  [cmdletbinding()]
  param([string] $DirectoryName)
  & dotnet pack ("""" + $DirectoryName + """") -c Release -o .\artifacts\packages; if($LASTEXITCODE -ne 0) { exit 1 }
}

<#
.SYNOPSIS
    Publishes a unit test project into an indexed folder for later execution.
.DESCRIPTION
    Builds and publishes a project as a unit test project. Published tests go in an indexed
    folder and can later be enumerated and executed.
.PARAMETER DirectoryName
    The path to the directory containing the project to build.
.PARAMETER Index
    The folder index under which the project should be published.
#>
function Publish-TestProject
{
  [cmdletbinding()]
  param([string] $DirectoryName, [int]$Index)

  # Publish to a numbered/indexed folder rather than the full test project name
  # because the package paths get long and start exceeding OS limitations.
  & dotnet publish ("""" + $DirectoryName + """") -c Release -o .\artifacts\tests\$Index; if($LASTEXITCODE -ne 0) { exit 2 }
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
function Remove-PathVariable
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