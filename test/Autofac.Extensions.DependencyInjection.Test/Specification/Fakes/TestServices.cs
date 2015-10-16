// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Extensions.DependencyInjection.Tests.Fakes
{
    public static class TestServices
    {
        public static IServiceCollection DefaultServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<IFakeService, FakeService>();
            services.AddTransient<IFakeMultipleService, FakeOneMultipleService>();
            services.AddTransient<IFakeMultipleService, FakeTwoMultipleService>();
            services.AddTransient<IFakeOuterService, FakeOuterService>();
            services.AddInstance<IFakeServiceInstance>(new FakeService() { Message = "Instance" });
            services.AddScoped<IFakeScopedService, FakeService>();
            services.AddSingleton<IFakeSingletonService, FakeService>();
            services.AddTransient<IDependOnNonexistentService, DependOnNonexistentService>();
            services.AddTransient<IFakeOpenGenericService<string>, FakeService>();
            services.AddTransient(typeof(IFakeOpenGenericService<>), typeof(FakeOpenGenericService<>));

            services.AddTransient<IFactoryService>(provider =>
            {
                var fakeService = provider.GetService<IFakeService>();
                return new TransientFactoryService
                {
                    FakeService = fakeService,
                    Value = 42
                };
            });

            services.AddScoped(provider =>
            {
                var fakeService = provider.GetService<IFakeService>();
                return new ScopedFactoryService
                {
                    FakeService = fakeService,
                };
            });
            services.AddScoped<ClassWithNestedReferencesToProvider>();

            services.AddTransient<ServiceAcceptingFactoryService, ServiceAcceptingFactoryService>();
            return services;
        }
    }
}