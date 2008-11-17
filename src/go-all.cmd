@echo off
@Resource\Nant\Nant.exe -buildfile:Autofac.build %*
@Resource\Nant\Nant.exe -buildfile:Autofac.build silverlight %*
@Resource\Nant\Nant.exe -buildfile:Autofac.build net20 %*