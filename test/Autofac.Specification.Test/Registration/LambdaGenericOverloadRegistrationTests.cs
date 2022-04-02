// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core;

namespace Autofac.Specification.Test.Registration;

#nullable enable

public class LambdaGenericOverloadRegistrationTests
{
    private class MyDep1
    {
    }

    private class MyDep2
    {
    }

    private class MyDep3
    {
    }

    private class MyDep4
    {
    }

    private class MyDep5
    {
    }

    private class MyDep6
    {
    }

    private class MyDep7
    {
    }

    private class MyDep8
    {
    }

    private class MyDep9
    {
    }

    private class MyDep10
    {
    }

    private class MyComponent
    {
        public MyComponent(MyDep1 d1)
        {
        }

        public MyComponent(MyDep1 d1, MyDep2 d2)
        {
        }

        public MyComponent(MyDep1 d1, MyDep2 d2, MyDep3 d3)
        {
        }

        public MyComponent(MyDep1 d1, MyDep2 d2, MyDep3 d3, MyDep4 d4)
        {
        }

        public MyComponent(MyDep1 d1, MyDep2 d2, MyDep3 d3, MyDep4 d4, MyDep5 d5)
        {
        }

        public MyComponent(MyDep1 d1, MyDep2 d2, MyDep3 d3, MyDep4 d4, MyDep5 d5, MyDep6 d6)
        {
        }

        public MyComponent(MyDep1 d1, MyDep2 d2, MyDep3 d3, MyDep4 d4, MyDep5 d5, MyDep6 d6, MyDep7 d7)
        {
        }

        public MyComponent(MyDep1 d1, MyDep2 d2, MyDep3 d3, MyDep4 d4, MyDep5 d5, MyDep6 d6, MyDep7 d7, MyDep8 d8)
        {
        }

        public MyComponent(MyDep1 d1, MyDep2 d2, MyDep3 d3, MyDep4 d4, MyDep5 d5, MyDep6 d6, MyDep7 d7, MyDep8 d8, MyDep9 d9)
        {
        }

        public MyComponent(MyDep1 d1, MyDep2 d2, MyDep3 d3, MyDep4 d4, MyDep5 d5, MyDep6 d6, MyDep7 d7, MyDep8 d8, MyDep9 d9, MyDep10 d10)
        {
        }
    }

    private class MyComponentWithParams
    {
        public MyComponentWithParams(MyDep1 dep1, string arg1, string arg2)
        {
            Arg1 = arg1;
            Arg2 = arg2;
        }

        public string Arg1 { get; }

        public string Arg2 { get; }
    }

    private static readonly Type[] _allDeps = new[]
    {
        typeof(MyDep1),
        typeof(MyDep2),
        typeof(MyDep3),
        typeof(MyDep4),
        typeof(MyDep5),
        typeof(MyDep6),
        typeof(MyDep7),
        typeof(MyDep8),
        typeof(MyDep9),
        typeof(MyDep10),
    };

    private ContainerBuilder GetBuilderWithDeps()
    {
        var builder = new ContainerBuilder();

        builder.RegisterTypes(_allDeps);

        return builder;
    }

    [Fact]
    public void RegisterLambdaWith1DependencyAndContext()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((IComponentContext c, MyDep1 dep1) => new MyComponent(dep1));

        var context = builder.Build();

        context.Resolve<MyComponent>();
    }

    [Fact]
    public void RegisterLambdaWith1Dependency()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((MyDep1 dep1) => new MyComponent(dep1));

        var context = builder.Build();

        context.Resolve<MyComponent>();
    }

    [Fact]
    public void RegisterLambdaWith2DependenciesAndContext()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((IComponentContext c, MyDep1 dep1, MyDep2 dep2) => new MyComponent(dep1, dep2));

        var context = builder.Build();

        context.Resolve<MyComponent>();
    }

    [Fact]
    public void RegisterLambdaWith2Dependencies()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((MyDep1 dep1, MyDep2 dep2) => new MyComponent(dep1, dep2));

        var context = builder.Build();

        context.Resolve<MyComponent>();
    }

    [Fact]
    public void RegisterLambdaWith3DependenciesAndContext()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((IComponentContext c, MyDep1 dep1, MyDep2 dep2, MyDep3 dep3) => new MyComponent(dep1, dep2, dep3));

        var context = builder.Build();

        context.Resolve<MyComponent>();
    }

    [Fact]
    public void RegisterLambdaWith3Dependencies()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((MyDep1 dep1, MyDep2 dep2, MyDep3 dep3) => new MyComponent(dep1, dep2, dep3));

        var context = builder.Build();

        context.Resolve<MyComponent>();
    }

    [Fact]
    public void RegisterLambdaWith4DependenciesAndContext()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((
            IComponentContext c,
            MyDep1 dep1,
            MyDep2 dep2,
            MyDep3 dep3,
            MyDep4 dep4) => new MyComponent(dep1, dep2, dep3, dep4));

        var context = builder.Build();

        context.Resolve<MyComponent>();
    }

    [Fact]
    public void RegisterLambdaWith4Dependencies()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((
            MyDep1 dep1,
            MyDep2 dep2,
            MyDep3 dep3,
            MyDep4 dep4) => new MyComponent(dep1, dep2, dep3, dep4));

        var context = builder.Build();

        context.Resolve<MyComponent>();
    }

    [Fact]
    public void RegisterLambdaWith5DependenciesAndContext()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((
            IComponentContext c,
            MyDep1 dep1,
            MyDep2 dep2,
            MyDep3 dep3,
            MyDep4 dep4,
            MyDep5 dep5) => new MyComponent(dep1, dep2, dep3, dep4, dep5));

        var context = builder.Build();

        context.Resolve<MyComponent>();
    }

    [Fact]
    public void RegisterLambdaWith5Dependencies()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((
            MyDep1 dep1,
            MyDep2 dep2,
            MyDep3 dep3,
            MyDep4 dep4,
            MyDep5 dep5) => new MyComponent(dep1, dep2, dep3, dep4, dep5));

        var context = builder.Build();

        context.Resolve<MyComponent>();
    }

    [Fact]
    public void RegisterLambdaWith6DependenciesAndContext()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((
            IComponentContext c,
            MyDep1 dep1,
            MyDep2 dep2,
            MyDep3 dep3,
            MyDep4 dep4,
            MyDep5 dep5,
            MyDep6 dep6) => new MyComponent(dep1, dep2, dep3, dep4, dep5, dep6));

        var context = builder.Build();

        context.Resolve<MyComponent>();
    }

    [Fact]
    public void RegisterLambdaWith6Dependencies()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((
            MyDep1 dep1,
            MyDep2 dep2,
            MyDep3 dep3,
            MyDep4 dep4,
            MyDep5 dep5,
            MyDep6 dep6) => new MyComponent(dep1, dep2, dep3, dep4, dep5, dep6));

        var context = builder.Build();

        context.Resolve<MyComponent>();
    }

    [Fact]
    public void RegisterLambdaWith7DependenciesAndContext()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((
            IComponentContext c,
            MyDep1 dep1,
            MyDep2 dep2,
            MyDep3 dep3,
            MyDep4 dep4,
            MyDep5 dep5,
            MyDep6 dep6,
            MyDep7 dep7) => new MyComponent(dep1, dep2, dep3, dep4, dep5, dep6, dep7));

        var context = builder.Build();

        context.Resolve<MyComponent>();
    }

    [Fact]
    public void RegisterLambdaWith7Dependencies()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((
            MyDep1 dep1,
            MyDep2 dep2,
            MyDep3 dep3,
            MyDep4 dep4,
            MyDep5 dep5,
            MyDep6 dep6,
            MyDep7 dep7) => new MyComponent(dep1, dep2, dep3, dep4, dep5, dep6, dep7));

        var context = builder.Build();

        context.Resolve<MyComponent>();
    }

    [Fact]
    public void RegisterLambdaWith8DependenciesAndContext()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((
            IComponentContext c,
            MyDep1 dep1,
            MyDep2 dep2,
            MyDep3 dep3,
            MyDep4 dep4,
            MyDep5 dep5,
            MyDep6 dep6,
            MyDep7 dep7,
            MyDep8 dep8) => new MyComponent(dep1, dep2, dep3, dep4, dep5, dep6, dep7, dep8));

        var context = builder.Build();

        context.Resolve<MyComponent>();
    }

    [Fact]
    public void RegisterLambdaWith8Dependencies()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((
            MyDep1 dep1,
            MyDep2 dep2,
            MyDep3 dep3,
            MyDep4 dep4,
            MyDep5 dep5,
            MyDep6 dep6,
            MyDep7 dep7,
            MyDep8 dep8) => new MyComponent(dep1, dep2, dep3, dep4, dep5, dep6, dep7, dep8));

        var context = builder.Build();

        context.Resolve<MyComponent>();
    }

    [Fact]
    public void RegisterLambdaWith9DependenciesAndContext()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((
            IComponentContext c,
            MyDep1 dep1,
            MyDep2 dep2,
            MyDep3 dep3,
            MyDep4 dep4,
            MyDep5 dep5,
            MyDep6 dep6,
            MyDep7 dep7,
            MyDep8 dep8,
            MyDep9 dep9) => new MyComponent(dep1, dep2, dep3, dep4, dep5, dep6, dep7, dep8, dep9));

        var context = builder.Build();

        context.Resolve<MyComponent>();
    }

    [Fact]
    public void RegisterLambdaWith9Dependencies()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((
            MyDep1 dep1,
            MyDep2 dep2,
            MyDep3 dep3,
            MyDep4 dep4,
            MyDep5 dep5,
            MyDep6 dep6,
            MyDep7 dep7,
            MyDep8 dep8,
            MyDep9 dep9) => new MyComponent(dep1, dep2, dep3, dep4, dep5, dep6, dep7, dep8, dep9));

        var context = builder.Build();

        context.Resolve<MyComponent>();
    }

    [Fact]
    public void RegisterLambdaWith10DependenciesAndContext()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((
            IComponentContext c,
            MyDep1 dep1,
            MyDep2 dep2,
            MyDep3 dep3,
            MyDep4 dep4,
            MyDep5 dep5,
            MyDep6 dep6,
            MyDep7 dep7,
            MyDep8 dep8,
            MyDep9 dep9,
            MyDep10 dep10) => new MyComponent(dep1, dep2, dep3, dep4, dep5, dep6, dep7, dep8, dep9, dep10));

        var context = builder.Build();

        context.Resolve<MyComponent>();
    }

    [Fact]
    public void RegisterLambdaWith10Dependencies()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((
            MyDep1 dep1,
            MyDep2 dep2,
            MyDep3 dep3,
            MyDep4 dep4,
            MyDep5 dep5,
            MyDep6 dep6,
            MyDep7 dep7,
            MyDep8 dep8,
            MyDep9 dep9,
            MyDep10 dep10) => new MyComponent(dep1, dep2, dep3, dep4, dep5, dep6, dep7, dep8, dep9, dep10));

        var context = builder.Build();

        context.Resolve<MyComponent>();
    }

    [Fact]
    public void RegisterLambdaWithTypedParameter()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((MyDep1 dep1, string arg) => new MyComponentWithParams(dep1, arg, arg));

        var result = builder.Build().Resolve<MyComponentWithParams>(TypedParameter.From("a"));

        Assert.Equal("a", result.Arg1);
        Assert.Equal("a", result.Arg2);
    }

    [Fact]
    public void RegisterLambdaWithNamedParameter()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((MyDep1 dep1, string arg1, string arg2) => new MyComponentWithParams(dep1, arg1, arg2));

        var result = builder.Build().Resolve<MyComponentWithParams>(new NamedParameter("arg2", "b"), new NamedParameter("arg1", "a"));

        Assert.Equal("a", result.Arg1);
        Assert.Equal("b", result.Arg2);
    }

    [Fact]
    public void RegisterLambdaWithMissingParameterThrows()
    {
        var builder = GetBuilderWithDeps();

        builder.Register((MyDep1 dep1, string arg) => new MyComponentWithParams(dep1, arg, arg));

        var container = builder.Build();

        Assert.Throws<DependencyResolutionException>(() => container.Resolve<MyComponentWithParams>());
    }

    [Theory]
    [MemberData(nameof(GetGenericOverloadTypeSets))]
    public void OverloadsAllCheckNullDelegate(Type[] types)
    {
        var builder = GetBuilderWithDeps();

        var registerMethod = GetRegisterMethod(types, withComponentContext: false);

        // Invoke generated delegate.
        AssertThrowsViaInvocation<ArgumentNullException>(() => registerMethod.Invoke(null, new[] { builder, null }));
    }

    [Theory]
    [MemberData(nameof(GetGenericOverloadTypeSets))]
    public void OverloadsWithContextAllCheckNullDelegate(Type[] types)
    {
        var builder = GetBuilderWithDeps();

        var registerMethod = GetRegisterMethod(types, withComponentContext: true);

        // Invoke generated delegate.
        AssertThrowsViaInvocation<ArgumentNullException>(() => registerMethod.Invoke(null, new[] { builder, null }));
    }

    private void AssertThrowsViaInvocation<TException>(Action testCode)
        where TException : Exception
    {
        Assert.Throws<TException>(() =>
        {
            try
            {
                testCode();
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException is not null)
                {
                    throw ex.InnerException;
                }

                throw;
            }
        });
    }

    /// <summary>
    /// Generate a concrete register overload from the given set of types, indicating whether you want the one with a component context
    /// at the start, or not.
    /// </summary>
    private MethodInfo GetRegisterMethod(Type[] types, bool withComponentContext)
    {
        static bool MethodDelegateFuncHasIComponentContext(MethodInfo method)
            => method.GetParameters().Last().ParameterType.GetGenericArguments().FirstOrDefault() == typeof(IComponentContext);

        var genericMethod = typeof(RegistrationExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                          .Where(x => x.Name == nameof(RegistrationExtensions.Register) &&
                                                                      x.GetGenericArguments().Length == types.Length + 1 &&
                                                                      MethodDelegateFuncHasIComponentContext(x) == withComponentContext)
                                                          .FirstOrDefault();

        var actualMethod = genericMethod!.MakeGenericMethod(types.Append(typeof(MyComponent)).ToArray());

        return actualMethod;
    }

    public static IEnumerable<object[]> GetGenericOverloadTypeSets()
    {
        // Return a set of type arrays, each one with an additional type in the set.
        for (var idx = 0; idx < _allDeps.Length; idx++)
        {
            yield return new[] { _allDeps.Take(idx + 1).ToArray() };
        }
    }
}
