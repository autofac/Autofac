// This software is part of the Autofac IoC container
// Copyright (c) 2010 Autofac Contributors
// http://autofac.org
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
// OF MERCHANTABILITY, FITNESS FOR A1 PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using Autofac.Integration.Wcf;
using NUnit.Framework;
using Autofac;

namespace Autofac.Tests.Integration.Wcf
{
    [TestFixture]
    public class AutofacRequestLifetimeScopeExtensionFixture
    {
        [Test(Description = "Attach should be a no-op.")]
        public void Attach_NullOwner()
        {
            var lifetime = new ContainerBuilder().Build().BeginLifetimeScope();
            var ext = new AutofacRequestLifetimeScopeExtension(lifetime);
            Assert.DoesNotThrow(() => ext.Attach(null));
        }

        [Test(Description = "Ensures the request lifetime can't be null.")]
        public void Ctor_NullRequestLifetime()
        {
            Assert.Throws<ArgumentNullException>(() => new AutofacRequestLifetimeScopeExtension(null));
        }

        [Test(Description = "Checks that the constructor sets the request lifetime property.")]
        public void Ctor_SetsRequestLifetime()
        {
            var lifetime = new ContainerBuilder().Build().BeginLifetimeScope();
            var ext = new AutofacRequestLifetimeScopeExtension(lifetime);
            Assert.AreSame(lifetime, ext.RequestLifetime, "The lifetime scope was not set by the constructor.");
        }

        [Test(Description = "Detach should be a no-op.")]
        public void Detach_NullOwner()
        {
            var lifetime = new ContainerBuilder().Build().BeginLifetimeScope();
            var ext = new AutofacRequestLifetimeScopeExtension(lifetime);
            Assert.DoesNotThrow(() => ext.Detach(null));
        }
    }
}
