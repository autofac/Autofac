// This software is part of the Autofac IoC container
// Copyright (c) 2007 Nicholas Blumhardt
// nicholas.blumhardt@gmail.com
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

namespace Autofac.Registrars
{
    /// <summary>
    /// A 'concrete' registrar creates IComponentRegistration
    /// instances that are registered with the container. This is necessary because
    /// some registration types (e.g. Generic) add registrations only on demand through
    /// IRegistrationSource and thus do not have unique names within the container.
    /// </summary>
    public interface IConcreteRegistrar : IConcreteRegistrar<IConcreteRegistrar>
    {
    }

    /// <summary>
    /// A 'concrete' registrar creates IComponentRegistration
    /// instances that are registered with the container. This is necessary because
    /// some registration types (e.g. Generic) add registrations only on demand through
    /// IRegistrationSource and thus do not have unique names within the container.
    /// </summary>
    /// <typeparam name="TSyntax"></typeparam>
    public interface IConcreteRegistrar<TSyntax> : IRegistrar<TSyntax>
        where TSyntax : IConcreteRegistrar<TSyntax>
    {
        /// <summary>
        /// Associate a name with the registration.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        TSyntax Named(string name);

        /// <summary>
        /// Associate services with the registration.
        /// </summary>
        /// <param name="services">The services that the registration will expose.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        TSyntax As(params Service[] services);
    }
}
