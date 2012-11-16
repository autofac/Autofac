@ECHO OFF
SETLOCAL

REM Comment this out to use the current environment variable value
SET DXROOT=%CD%\..\

ECHO *
ECHO * Building .NET Framework reflection data files using tools in %DXROOT%
ECHO *

CD %DXROOT%\Data

REM Use the command line overrides if specified

REM This isn't supported yet, MRefBuilder tends to not find dependencies or crash on different frameworks.
REM This will be addressed later if there is a need for it.

REM IF NOT '%1'=='' SET FrameworkPlatform=%1
REM IF NOT '%2'=='' SET FrameworkVersion=%2

"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "BuildReflectionData.proj"
IF ERRORLEVEL 1 GOTO BuildFailed

ECHO *
ECHO * The reflection data has been built successfully
ECHO *

GOTO Done

:BuildFailed

ECHO *
ECHO * Build failed!
ECHO *

:Done
ENDLOCAL
