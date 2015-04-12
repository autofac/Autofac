@ECHO OFF

PUSHD %~dp0

:dnvminstall
SETLOCAL EnableDelayedExpansion 
where dnvm
IF %ERRORLEVEL% neq 0 (
    @powershell -NoProfile -ExecutionPolicy unrestricted -Command "&{$Branch='dev';iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}"
    SET PATH=!PATH!;!userprofile!\.dnx\bin
    SET DNX_HOME=!USERPROFILE!\.dnx
    GOTO install
)

:install
CALL dnvm install 1.0.0-beta5-11511 -r CoreCLR
CALL dnvm install 1.0.0-beta5-11511 -r CLR

:restore
CALL dnu restore src\Autofac
IF %errorlevel% neq 0 EXIT /b %errorlevel%

CALL dnu restore src\Autofac.Dnx
IF %errorlevel% neq 0 EXIT /b %errorlevel%

CALL dnu restore test\Autofac.Test
IF %errorlevel% neq 0 EXIT /b %errorlevel%

CALL dnu restore test\Autofac.Dnx.Test
IF %errorlevel% neq 0 EXIT /b %errorlevel%

:pack
SETLOCAL ENABLEEXTENSIONS
IF ERRORLEVEL 1 ECHO Unable to enable extensions
IF DEFINED APPVEYOR_BUILD_NUMBER (SET DNX_BUILD_VERSION=%APPVEYOR_BUILD_NUMBER%) ELSE (SET DNX_BUILD_VERSION=1)
ECHO DNX_BUILD_VERSION=%DNX_BUILD_VERSION%

CALL dnu pack src\Autofac --configuration Release --out artifacts\packages
IF %errorlevel% neq 0 EXIT /b %errorlevel%

CALL dnu pack src\Autofac.Dnx --configuration Release --out artifacts\packages
IF %errorlevel% neq 0 EXIT /b %errorlevel%

:test
CALL dnx test\Autofac.Test test
IF %errorlevel% neq 0 EXIT /b %errorlevel%

CALL dnx test\Autofac.Dnx.Test test
IF %errorlevel% neq 0 EXIT /b %errorlevel%

CALL dnvm use 1.0.0-beta5-11511 -r CoreCLR

CALL dnx test\Autofac.Test test
IF %errorlevel% neq 0 EXIT /b %errorlevel%

CALL dnx test\Autofac.Dnx.Test test
IF %errorlevel% neq 0 EXIT /b %errorlevel%

POPD