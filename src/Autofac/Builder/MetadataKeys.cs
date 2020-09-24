// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Builder
{
    /// <summary>
    /// Internal metadata keys.
    /// </summary>
    internal static class MetadataKeys
    {
        /// <summary>
        /// Key containing the registration order.
        /// </summary>
        internal const string RegistrationOrderMetadataKey = "__RegistrationOrder";

        /// <summary>
        /// Key containing a value indicating whether the registration has been auto activated.
        /// </summary>
        internal const string AutoActivated = "__AutoActivated";

        /// <summary>
        /// Key containing a value indicating whether the registration should be started on activation.
        /// </summary>
        internal const string StartOnActivatePropertyKey = "__StartOnActivate";

        /// <summary>
        /// Key containing the container build options.
        /// </summary>
        internal const string ContainerBuildOptions = "__ContainerBuildOptions";

        /// <summary>
        /// Event handler for <see cref="Autofac.Core.Registration.ComponentRegistryBuilder.Registered"/>.
        /// </summary>
        internal const string RegisteredPropertyKey = "__RegisteredKey";

        /// <summary>
        /// Event handler for <see cref="Autofac.Core.Registration.ComponentRegistryBuilder.RegistrationSourceAdded"/>.
        /// </summary>
        internal const string RegistrationSourceAddedPropertyKey = "__RegistrationSourceAddedKey";
    }
}
