@echo off
IF "%1"=="" GOTO HELP
pushd bin\%1
SecAnnotate.exe /verbose AutofacContrib.Multitenant.dll /r:Castle.Core.dll /r:Autofac.dll
popd
GOTO END

:HELP
echo Executes SecAnnotate against the AutofacContrib.Multitenant library.
echo Security [Debug or Release]
echo like
echo Security Debug

:END