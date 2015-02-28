@powershell -NoProfile -ExecutionPolicy unrestricted -Command "iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/master/kvminstall.ps1'))"
call kvm upgrade
call kpm restore
call kpm pack src\Autofac --configuration Release
call kpm pack src\Autofac.AspNet --configuration Release
call kpm build test\Autofac.Test --configuration Release
pushd test\Autofac.Test
call k test
popd