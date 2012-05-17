using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel;

namespace Autofac.Extras.EnterpriseLibraryConfigurator
{
    /// <summary>
    /// Visitor for resolving object construction parameters from
    /// <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ParameterValue"/>
    /// registrations during container configuration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When registering a dependency from Enterprise Library, you get an instance
    /// of <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.TypeRegistration"/>.
    /// To construct an instance of the type described by the registration,
    /// you need to add the registration to your <see cref="Autofac.ContainerBuilder"/>
    /// with a corresponding set of <see cref="Autofac.Core.Parameter"/>
    /// values.
    /// </para>
    /// <para>
    /// To convert the constructor parameters from Enterprise Library format
    /// to Autofac format, you need to look at each of the
    /// <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.TypeRegistration.ConstructorParameters"/>
    /// and create a corresponding <see cref="Autofac.Core.Parameter"/> that
    /// can be added to the dependency registration in the Autofac container.
    /// </para>
    /// <para>
    /// This class takes advantage of the
    /// <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ParameterValueVisitor.Visit"/>
    /// method to determine the type of the parameter and creates a corresponding
    /// <see cref="Autofac.Core.ResolvedParameter"/> with the appropriate value.
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Parameter Type</term>
    /// <description>Resolved Parameter Value</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ConstantParameterValue"/></term>
    /// <description>
    /// The exact provided constant value.
    /// (<see cref="Autofac.Extras.EnterpriseLibraryConfigurator.AutofacParameterBuilderVisitor.VisitConstantParameterValue"/>)
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ContainerResolvedEnumerableParameter"/></term>
    /// <description>
    /// A lambda that builds a generic list of the specified type containing
    /// the set of resolved named service instances.
    /// (<see cref="Autofac.Extras.EnterpriseLibraryConfigurator.AutofacParameterBuilderVisitor.VisitEnumerableParameterValue"/>)
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ContainerResolvedParameter"/></term>
    /// <description>
    /// If the parameter is a named parameter, the named service is resolved;
    /// otherwise the typed service is resolved.
    /// (<see cref="Autofac.Extras.EnterpriseLibraryConfigurator.AutofacParameterBuilderVisitor.VisitResolvedParameterValue"/>)
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// After calling the
    /// <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ParameterValueVisitor.Visit"/>
    /// method on this visitor, the <see cref="Autofac.Extras.EnterpriseLibraryConfigurator.AutofacParameterBuilderVisitor.AutofacParameter"/>
    /// property will be set with the output of the transformation. You can use
    /// this in conjunction with <see cref="Autofac.RegistrationExtensions.WithParameters{TLimit, TReflectionActivatorData, TStyle}"/>
    /// to add the parameters to the component registration.
    /// </para>
    /// <para>
    /// Normally this class will not be used by itself. Instead, consider using
    /// it in conjunction with the <see cref="Autofac.Extras.EnterpriseLibraryConfigurator.AutofacContainerConfigurator"/>
    /// and the Enterprise Library registration extension methods in
    /// <see cref="Autofac.Extras.EnterpriseLibraryConfigurator.EnterpriseLibraryRegistrationExtensions"/>.
    /// </para>
    /// <para>
    /// For more information on how type registrations are created and the possible
    /// values of constant and resolved parameters, see
    /// <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.TypeRegistration"/>,
    /// which is the entry point for creating parameter values, and
    /// <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.Container"/>,
    /// which is the placeholder class used by the registration mechanism for
    /// signifying that a parameter value is resolved rather than constant.
    /// </para>
    /// </remarks>
    /// <seealso cref="Autofac.Extras.EnterpriseLibraryConfigurator.EnterpriseLibraryRegistrationExtensions"/>
    /// <seealso cref="Autofac.Extras.EnterpriseLibraryConfigurator.AutofacContainerConfigurator"/>
    public class AutofacParameterBuilderVisitor : ParameterValueVisitor
    {
        /// <summary>
        /// Storage for the reflected parameter information that should receive
        /// the resolved value from <see cref="Autofac.Extras.EnterpriseLibraryConfigurator.AutofacParameterBuilderVisitor.AutofacParameter"/>
        /// </summary>
        private readonly ParameterInfo _methodParameter;

        /// <summary>
        /// Gets the generated <see cref="Autofac.Core.Parameter"/>
        /// after executing <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ParameterValueVisitor.Visit"/>.
        /// </summary>
        /// <value>
        /// A <see cref="Autofac.Core.Parameter"/> that corresponds to the parameter
        /// passed in during construction. This value will be <see langword="null" />
        /// if requested prior to execution of
        /// <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ParameterValueVisitor.Visit"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// This value will always end up being a <see cref="Autofac.Core.ResolvedParameter"/>
        /// rather than some other type like a <see cref="Autofac.PositionalParameter"/>
        /// because we need to be very specific about the constructor parameter
        /// position, but flexible on the <see cref="System.Type"/> of the argument.
        /// </para>
        /// <para>
        /// The <see cref="Autofac.PositionalParameter"/>, for example,
        /// requires that the <see cref="System.Type"/> of the parameter value
        /// match exactly with the type at the given position, which isn't
        /// always true for Enterprise Library.
        /// </para>
        /// <para>
        /// For example, say you have an object that has two constructors, like:
        /// </para>
        /// <code lang="C#">
        /// public MyObject(ISomeService a, string b) { }
        /// public MyObject(string a, string b) { }
        /// </code>
        /// <para>
        /// If you register an object that implements <c>ISomeService</c> (like
        /// "<c>SomeServiceImpl</c>") and set it in a <see cref="Autofac.PositionalParameter"/>
        /// at position 0, Autofac searches through the constructors and says it
        /// can't find a constructor that takes a <c>SomeServiceImpl</c> as
        /// the first parameter, even though it matches the first constructor.
        /// </para>
        /// <para>
        /// However, if you use <see cref="Autofac.Core.ResolvedParameter"/>,
        /// you can match on the reflected parameter information rather than
        /// position and provide an appropriate value, which allows the above
        /// example to work correctly.
        /// </para>
        /// </remarks>
        public Parameter AutofacParameter { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacParameterBuilderVisitor"/> class.
        /// </summary>
        /// <param name="methodParameter">
        /// The reflected information about the method parameter that will be visited.
        /// </param>
        /// <remarks>
        /// <para>
        /// When <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ParameterValueVisitor.Visit"/>
        /// is called, the value passed in to be visited should be the value
        /// that goes in the constructor parameter indicated by <paramref name="methodParameter" />.
        /// They need to match because the generated
        /// <see cref="Autofac.Core.Parameter"/> in
        /// <see cref="Autofac.Extras.EnterpriseLibraryConfigurator.AutofacParameterBuilderVisitor.AutofacParameter"/>
        /// will match the reflected information about the parameter to the value
        /// it has to resolve.
        /// </para>
        /// </remarks>
        public AutofacParameterBuilderVisitor(ParameterInfo methodParameter)
        {
            if (methodParameter == null)
            {
                throw new ArgumentNullException("methodParameter");
            }
            this._methodParameter = methodParameter;
        }

        /// <summary>
        /// Creates a <see cref="Autofac.Core.ResolvedParameter"/> based on the
        /// provided resolution lambda.
        /// </summary>
        /// <param name="resolution">
        /// The function that resolves to the value that should be in the parameter.
        /// </param>
        /// <returns>
        /// A <see cref="Autofac.Core.ResolvedParameter"/> attached to the parameter
        /// passed into the visitor constructor that will resolve via <paramref name="resolution" />.
        /// </returns>
        private ResolvedParameter CreateResolvedParameter(Func<ParameterInfo, IComponentContext, object> resolution)
        {
            return new ResolvedParameter(
                (pi, context) => pi == this._methodParameter,
                resolution);
        }

        /// <summary>
        /// The method called when a
        /// <see cref="T:Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ConstantParameterValue"/>
        /// object is visited.
        /// </summary>
        /// <param name="parameterValue">
        /// The <see cref="T:Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ConstantParameterValue"/> to process.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method creates a <see cref="Autofac.Core.ResolvedParameter"/>
        /// based on the constant value in <paramref name="parameterValue" />
        /// and ties it to the parameter passed in during construction.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="parameterValue" /> is <see langword="null" />.
        /// </exception>
        protected override void VisitConstantParameterValue(ConstantParameterValue parameterValue)
        {
            if (parameterValue == null)
            {
                throw new ArgumentNullException("parameterValue");
            }
            this.AutofacParameter = this.CreateResolvedParameter((pi, context) => parameterValue.Value);
        }

        /// <summary>
        /// The method called when a
        /// <see cref="T:Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ContainerResolvedEnumerableParameter"/>
        /// object is visited.
        /// </summary>
        /// <param name="parameterValue">
        /// The <see cref="T:Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ContainerResolvedEnumerableParameter"/> to process.
        /// </param>
        /// <remarks>
        /// <para>
        /// Resolved enumerables in Enterprise Library are always made up of
        /// a list of individual named resolutions - there is no notion of a
        /// typed auto-collection resolution the way there is in Autofac.
        /// </para>
        /// <para>
        /// As such, the <see cref="Autofac.Core.ResolvedParameter"/> lambda
        /// this method generates is a loop over the set of resolved named
        /// typed registrations to build them manually into a list of the
        /// type specified.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="parameterValue" /> is <see langword="null" />.
        /// </exception>
        protected override void VisitEnumerableParameterValue(ContainerResolvedEnumerableParameter parameterValue)
        {
            if (parameterValue == null)
            {
                throw new ArgumentNullException("parameterValue");
            }
            this.AutofacParameter = this.CreateResolvedParameter(
                (pi, context) =>
                {
                    var listType = typeof(List<>).MakeGenericType(parameterValue.ElementType);
                    var list = Activator.CreateInstance(listType) as IList;
                    foreach (var name in parameterValue.Names)
                    {
                        list.Add(context.ResolveNamed(name, parameterValue.ElementType));
                    }
                    return list;
                });
        }

        /// <summary>
        /// The method called when a
        /// <see cref="T:Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ContainerResolvedParameter"/>
        /// object is visited.
        /// </summary>
        /// <param name="parameterValue">
        /// The <see cref="T:Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ContainerResolvedParameter"/> to process.
        /// </param>
        /// <remarks>
        /// <para>
        /// If the <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ContainerResolvedParameter.Name"/>
        /// is not set on the <paramref name="parameterValue" />, the value
        /// is assumed to be a typed registration. If the
        /// <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ContainerResolvedParameter.Name"/>
        /// is present, the value is assumed to be both typed and named.
        /// </para>
        /// <para>
        /// Either way, a <see cref="Autofac.Core.ResolvedParameter"/> will
        /// be created with a lambda that resolves the typed (and possibly named,
        /// as the case may be) service for the parameter passed in during construction.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="parameterValue" /> is <see langword="null" />.
        /// </exception>
        protected override void VisitResolvedParameterValue(ContainerResolvedParameter parameterValue)
        {
            if (parameterValue == null)
            {
                throw new ArgumentNullException("parameterValue");
            }
            if (!String.IsNullOrEmpty(parameterValue.Name))
            {
                this.AutofacParameter = this.CreateResolvedParameter((pi, context) => context.ResolveNamed(parameterValue.Name, parameterValue.Type));
            }
            else
            {
                this.AutofacParameter = this.CreateResolvedParameter((pi, context) => context.Resolve(parameterValue.Type));
            }
        }
    }
}
