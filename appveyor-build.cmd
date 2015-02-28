kvm upgrade
kpm restore
kpm pack src\Autofac --configuration Release
kpm pack src\Autofac.AspNet --configuration Release
kpm build test\Autofac.Test --configuration Release