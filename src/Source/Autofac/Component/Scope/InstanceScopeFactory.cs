using System;
using System.Globalization;

namespace Autofac.Component.Scope
{
	/// <summary>
	/// Factory for IScope entities that extends the InstanceScope class.
	/// </summary>
	static class InstanceScopeFactory
	{
		/// <summary>
		/// Create an IScope corresponding to the provided enum value.
		/// </summary>
		/// <param name="scope">Enum value representing a scope.</param>
		/// <returns>An equivalent IScope.</returns>
		public static IScope ToIScope(this InstanceScope scope)
		{
			switch (scope)
			{
				case InstanceScope.Container:
					return new ContainerScope();
				case InstanceScope.Factory:
					return new FactoryScope();
				case InstanceScope.Singleton:
					return new SingletonScope();
				default:
					throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,
						InstanceScopeFactoryResources.UnknownScope, scope));
			}
		}
	}
}
