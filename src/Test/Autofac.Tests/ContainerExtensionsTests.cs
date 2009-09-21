//// This software is part of the Autofac IoC container
//// Copyright (c) 2007 - 2009 Autofac Contributors
//// http://autofac.org
//// 
//// Permission is hereby granted, free of charge, to any person
//// obtaining a copy of this software and associated documentation
//// files (the "Software"), to deal in the Software without
//// restriction, including without limitation the rights to use,
//// copy, modify, merge, publish, distribute, sublicense, and/or sell
//// copies of the Software, and to permit persons to whom the
//// Software is furnished to do so, subject to the following
//// conditions:
//// 
//// The above copyright notice and this permission notice shall be
//// included in all copies or substantial portions of the Software.
//// 
//// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
//// OF MERCHANTABILITY, FITNESS FOR A1 PARTICULAR PURPOSE AND
//// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
//// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
//// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
//// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
//// OTHER DEALINGS IN THE SOFTWARE.

//using Autofac.Builder;
//using NUnit.Framework;

//namespace Autofac.Tests
//{
//    [TestFixture]
//    public sealed class ContainerExtensionsTests
//    {
//        // ReSharper disable InconsistentNaming

//        interface IMyService { }

//        sealed class MyComponent : IMyService { }

//        [Test]
//        public void Build_case()
//        {
//            using (var container = new Container())
//            {
//                container.Build(b => b.Register<MyComponent>().As<IMyService>());

//                var component = container.Resolve<IMyService>();
//                Assert.IsTrue(component is MyComponent);
//            }
//        }

//        [Test]
//        public void Pre_build_case()
//        {
//            var builder = new ContainerBuilder();
//            builder.Register<MyComponent>().As<IMyService>();

//            using (var container = new Container())
//            {
//                builder.Build(container);
//                var component = container.Resolve<IMyService>();
//                Assert.IsTrue(component is MyComponent);
//            }
//        }
//    }
//}