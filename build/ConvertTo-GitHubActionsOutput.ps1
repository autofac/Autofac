<#
.SYNOPSIS
    Converts an MSBuild issues log into build output for GitHub Actions.
.DESCRIPTION
    Rather than simply failing the build and having to figure out what the error
    was, this script takes the output from an MSBuild execution and converts
    warnings/errors into GitHub Actions format for easier readability.
.PARAMETER Path
    Path to the MSBuild issues log.
.EXAMPLE
    ConvertTo-GitHubActionsOutput -Path ./artifacts/dotnet-build-issues.log

    Converts the default log output into GitHub Actions format.
#>
[CmdletBinding(SupportsShouldProcess = $False)]
Param(
    [Parameter(Mandatory = $True)]
    [string]
    $Path
)

Begin {
    If (-not (Test-Path $Path)) {
        Exit 0
    }
}

Process {
    Get-Content $Path | ForEach-Object {
        If ($_ -match '^(?:\s+\d+>)?([^\s].*)\((\d+|\d+,\d+|\d+,\d+,\d+,\d+)\)\s*:\s+(error|warning)\s+(\w{1,2}\d+)\s*:\s*(.*)$') {
            $File = $Matches[1]
            $Severity = $Matches[3]
            If ($Severity -ne 'warning') {
                $Severity = 'error'
            }
            $Code = $Matches[4]
            $Message = $Matches[5].Trim()
            $LineNumberData = $Matches[2]
            $Col = 0
            $LineNumber = $LineNumberData
            If ($LineNumberData.IndexOf(',') -ge 0) {
                $Col = $LineNumberData -replace '[^,]+,',''
                $LineNumber = $LineNumberData -replace ',[^,]+',''
            }

            Write-Host "::$Severity file=$File,line=$LineNumber,col=$Col,title=$Code::$Message"
        }
    }
}
