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
using Autofac;
using Autofac.Integration.Wcf;
using NUnit.Framework;

namespace UnitTests.Autofac.Integration.Wcf
{
    [TestFixture]
    public class ContainerProviderFixture
    {
        [Test(Description = "Ensures the application container can't be null.")]
        public void Ctor_NullApplicationContainer()
        {
            Assert.Throws<ArgumentNullException>(() => new ContainerProvider(null));
        }

        [Test(Description = "Checks that the constructor sets the application container property.")]
        public void Ctor_SetsApplicationContainer()
        {
            var container = new ContainerBuilder().Build();
            var cp = new ContainerProvider(container);
            Assert.AreSame(container, cp.ApplicationContainer, "The application container was not set by the constructor.");
        }
    }
}
