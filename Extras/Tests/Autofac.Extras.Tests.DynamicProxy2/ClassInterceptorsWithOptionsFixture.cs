using Autofac.Extras.DynamicProxy2;
using Castle.DynamicProxy;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Autofac.Extras.Tests.DynamicProxy2
{

    [TestFixture]
    public class ClassInterceptorsWithOptionsFixture
    {
        [Intercept(typeof(AddOneInterceptor))]
        [Intercept(typeof(AddTenInterceptor))]
        public class C
        {
            public int I { get; set; }
            public int J { get; set; }

            public C(int i)
            {
                I = J = i;
            }

            public virtual int GetI()
            {
                return I;
            }

            public virtual int GetJ()
            {
                return J;
            }
        }

        public class D
        {
            public int I { get; set; }
            public int J { get; set; }

            public D(int i)
            {
                I = J = i;
            }

            public virtual int GetI()
            {
                return I;
            }

            public virtual int GetJ()
            {
                return J;
            }
        }

        class InterceptOnlyJ : IProxyGenerationHook
        {

            public void MethodsInspected()
            {
            }

            public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
            {
            }

            public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
            {
                return methodInfo.Name.Equals("GetJ");
            }

        }

        class MyInterceptorSelector : IInterceptorSelector
        {
            public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
            {
                var result = method.Name == "GetI"
                    ? interceptors.OfType<AddOneInterceptor>().ToArray<IInterceptor>()
                    : interceptors.OfType<AddTenInterceptor>().ToArray<IInterceptor>();

                if (result.Length == 0)
                    throw new InvalidOperationException("No interceptors for method " + method.Name);

                return result;
            }
        }

        class AddOneInterceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                invocation.Proceed();
                if (invocation.Method.Name == "GetI" || invocation.Method.Name == "GetJ")
                    invocation.ReturnValue = 1 + (int)invocation.ReturnValue;
            }
        }

        class AddTenInterceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                invocation.Proceed();
                if (invocation.Method.Name == "GetJ")
                    invocation.ReturnValue = 10 + (int)invocation.ReturnValue;
            }
        }

        [Test]
        public void CanCreateMixinWithClassInterceptors()
        {
            var options = new ProxyGenerationOptions();
            options.AddMixinInstance(new Dictionary<int, int>());

            var builder = new ContainerBuilder();
            builder.RegisterType<C>().EnableClassInterceptors(options);
            builder.RegisterType<AddOneInterceptor>();
            builder.RegisterType<AddTenInterceptor>();
            var container = builder.Build();
            var i = 10;
            var cpt = container.Resolve<C>(TypedParameter.From(i));

            var dict = cpt as IDictionary<int, int>;

            Assert.IsNotNull(dict);

            dict.Add(1, 2);

            Assert.AreEqual(2, dict[1]);

            dict.Clear();

            Assert.IsEmpty(dict);
        }

        [Test]
        public void CanInterceptOnlySpecificMethods()
        {
            var options = new ProxyGenerationOptions(new InterceptOnlyJ());

            var builder = new ContainerBuilder();
            builder.RegisterType<C>().EnableClassInterceptors(options);
            builder.RegisterType<AddOneInterceptor>();
            builder.RegisterType<AddTenInterceptor>();
            var container = builder.Build();
            var i = 10;
            var cpt = container.Resolve<C>(TypedParameter.From(i));

            Assert.AreEqual(i, cpt.GetI());
            Assert.AreEqual(i + 11, cpt.GetJ());
        }

        [Test]
        public void CanInterceptMethodsWithSpecificInterceptors()
        {
            var options = new ProxyGenerationOptions { Selector = new MyInterceptorSelector() };

            var builder = new ContainerBuilder();
            builder.RegisterType<C>().EnableClassInterceptors(options);
            builder.RegisterType<AddOneInterceptor>();
            builder.RegisterType<AddTenInterceptor>();
            var container = builder.Build();
            var i = 10;
            var cpt = container.Resolve<C>(TypedParameter.From(i));

            Assert.AreEqual(i + 1, cpt.GetI());
            Assert.AreEqual(i + 10, cpt.GetJ());
        }
    }
}

