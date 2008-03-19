// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using NUnit.Framework;
using Autofac.Component;
using Autofac.Component.Activation;
using Autofac.Component.Scope;
using System.Collections.Generic;

namespace Autofac.Tests
{
	[TestFixture]
	public class ComponentRegisteredEventArgsFixture
	{
		[Test]
		public void ConstructorSetsProperties()
		{
			var container = new Container();
			var registration = CreateRegistration();
			var args = new ComponentRegisteredEventArgs(container, registration);
			Assert.AreSame(container, args.Container);
			Assert.AreSame(registration, args.ComponentRegistration);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullContainerDetected()
		{
			var registration = CreateRegistration();
			var args = new ComponentRegisteredEventArgs(null, registration);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullRegistrationDetected()
		{
			var container = new Container();
			var args = new ComponentRegisteredEventArgs(container, null);
		}
		
		private IComponentRegistration CreateRegistration()
		{
			return new Registration(
                new Descriptor(
                    new UniqueService(), 
	    			new Service[0],
                    typeof(object)),
				new ReflectionActivator(typeof(object)), 
				new SingletonScope(),
				InstanceOwnership.Container);
		}
	}
}
