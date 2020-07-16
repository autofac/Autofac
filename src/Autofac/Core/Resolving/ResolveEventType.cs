using Autofac.Builder;

namespace Autofac.Core.Resolving.Middleware
{
    /// <summary>
    /// Defines the standard set of in-built resolve events, executed by the <see cref="CoreEventMiddleware"/>.
    /// </summary>
    public enum ResolveEventType
    {
        /// <summary>
        /// Event type for the OnPreparing event registered by <see cref="IRegistrationBuilder{TLimit, TActivatorData, TRegistrationStyle}.OnPreparing"/>.
        /// </summary>
        OnPreparing,

        /// <summary>
        /// Event type for the OnActivating event registered by <see cref="IRegistrationBuilder{TLimit, TActivatorData, TRegistrationStyle}.OnActivating(System.Action{IActivatingEventArgs{TLimit}})"/>.
        /// </summary>
        OnActivating,

        /// <summary>
        /// Event type for the OnActivated event registered by <see cref="IRegistrationBuilder{TLimit, TActivatorData, TRegistrationStyle}.OnActivated(System.Action{IActivatedEventArgs{TLimit}})"/>.
        /// </summary>
        OnActivated,

        /// <summary>
        /// Event type for the OnRelease event registered by <see cref="RegistrationExtensions.OnRelease"/>
        /// </summary>
        OnRelease
    }
}
