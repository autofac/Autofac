using Autofac;
using Autofac.Component;
using Autofac.Component.Activation;
using Autofac.Component.Scope;
using NMock2;

namespace NMock2
{
	/// <summary> Resolves unknown interfaces and Mocks using the <see cref="Mockery"/> from the scope. </summary>
	class NMockRegistrationHandler : IRegistrationSource
	{
		public bool TryGetRegistration(Service service, out IComponentRegistration registration)
		{
			registration = null;

			var typedService = service as TypedService;
			if ((typedService == null) || (!typedService.ServiceType.IsInterface))
				return false;

			var descriptor = new Descriptor(
				new UniqueService(),
				new[] { service },
				typedService.ServiceType);
			
			registration = new Registration(
				descriptor,
				new DelegateActivator((c, p) => c.Resolve<Mockery>().NewMock(typedService.ServiceType)),
				new ContainerScope(),
				InstanceOwnership.Container);
			return true;
		}
	}
}