@echo off

pushd Build\Library
SecAnnotate.exe /verbose Autofac.dll Autofac.Configuration.dll Autofac.Integration.Mef.dll Autofac.Integration.Mvc.dll Autofac.Integration.Wcf.dll Autofac.Integration.Web.dll
popd