// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Features.Decorators;

// The test project leaves <Nullable> unset, and SDK versions disagree about which nullable
// diagnostics that produces - the CI build reports CS8600/CS8620 here where a local build reports
// nothing. Opting this file into an explicit context makes the analysis the same everywhere.
#nullable enable

namespace Autofac.Test.Features.Decorators;

/// <summary>
/// Tests for the builder returned by the <c>RegisterDecorator</c> overloads, which allows a
/// decorator registration to be configured after it has been added to the container builder.
/// </summary>
public class DecoratorRegistrationBuilderTests
{
    private const string MetadataKey = "decorator-metadata";

    private interface IDecoratedService
    {
        string Name
        {
            get;
        }
    }

    [Fact]
    public void RegisterDecoratorReturnsABuilder()
    {
        var builder = new ContainerBuilder();

        var fromGeneric = builder.RegisterDecorator<DecoratorA, IDecoratedService>();
        var fromType = builder.RegisterDecorator(typeof(DecoratorB), typeof(IDecoratedService));
        var fromLambda = builder.RegisterDecorator<IDecoratedService>((_, _, inner) => new DecoratorA(inner));

        // The type argument each overload closes over is part of the public API. The non-generic
        // overload deliberately uses object, matching the precedent set by RegisterType(Type)
        // returning IRegistrationBuilder<object, ...>.
        Assert.IsAssignableFrom<IDecoratorRegistrationBuilder<IDecoratedService>>(fromGeneric);
        Assert.IsAssignableFrom<IDecoratorRegistrationBuilder<object>>(fromType);
        Assert.IsAssignableFrom<IDecoratorRegistrationBuilder<IDecoratedService>>(fromLambda);
    }

    [Fact]
    public void PipelineMiddlewareWrapsTheDecoratorInstance()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Implementor>().As<IDecoratedService>();
        builder.RegisterDecorator<DecoratorA, IDecoratedService>()
            .ConfigurePipeline(UseWrapper);

        var container = builder.Build();

        var service = container.Resolve<IDecoratedService>();

        // The wrapper is applied to the decorator, not to the component being decorated.
        Assert.Equal("Wrapper(A(Implementor))", service.Name);
    }

    [Fact]
    public void PipelineMiddlewareOnlyAppliesToTheDecoratorItWasConfiguredOn()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Implementor>().As<IDecoratedService>();
        builder.RegisterDecorator<DecoratorA, IDecoratedService>();
        builder.RegisterDecorator<DecoratorB, IDecoratedService>();

        var container = builder.Build();

        Assert.Equal("B(A(Implementor))", container.Resolve<IDecoratedService>().Name);
    }

    [Fact]
    public void PipelineMiddlewareOnTheOutermostDecoratorWrapsEverything()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Implementor>().As<IDecoratedService>();
        builder.RegisterDecorator<DecoratorA, IDecoratedService>();
        builder.RegisterDecorator<DecoratorB, IDecoratedService>()
            .ConfigurePipeline(UseWrapper);

        var container = builder.Build();

        Assert.Equal("Wrapper(B(A(Implementor)))", container.Resolve<IDecoratedService>().Name);
    }

    [Fact]
    public void PipelineMiddlewareOnTheInnermostDecoratorWrapsOnlyThatDecorator()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Implementor>().As<IDecoratedService>();
        builder.RegisterDecorator<DecoratorA, IDecoratedService>()
            .ConfigurePipeline(UseWrapper);
        builder.RegisterDecorator<DecoratorB, IDecoratedService>();

        var container = builder.Build();

        Assert.Equal("B(Wrapper(A(Implementor)))", container.Resolve<IDecoratedService>().Name);
    }

    [Fact]
    public void PipelineMiddlewareRunsAgainstTheDecoratorServiceAndRegistration()
    {
        Service? capturedService = null;
        IComponentRegistration? capturedRegistration = null;

        var builder = new ContainerBuilder();
        builder.RegisterType<Implementor>().As<IDecoratedService>();
        builder.RegisterDecorator<DecoratorA, IDecoratedService>()
            .ConfigurePipeline(p => p.Use(PipelinePhase.Activation, MiddlewareInsertionMode.StartOfPhase, (ctx, next) =>
            {
                next(ctx);
                capturedService = ctx.Service;
                capturedRegistration = ctx.Registration;
            }));

        var container = builder.Build();
        container.Resolve<IDecoratedService>();

        var decoratorService = Assert.IsType<DecoratorService>(capturedService);
        Assert.Equal(typeof(IDecoratedService), decoratorService.ServiceType);
        Assert.Equal(typeof(DecoratorA), capturedRegistration!.Activator.LimitType);
    }

    [Fact]
    public void MetadataIsVisibleOnTheDecoratorRegistration()
    {
        IComponentRegistration? capturedRegistration = null;

        var builder = new ContainerBuilder();
        builder.RegisterType<Implementor>().As<IDecoratedService>();
        builder.RegisterDecorator<DecoratorA, IDecoratedService>()
            .WithMetadata(MetadataKey, "the-value")
            .ConfigurePipeline(p => p.Use(PipelinePhase.Activation, (ctx, next) =>
            {
                next(ctx);
                capturedRegistration = ctx.Registration;
            }));

        var container = builder.Build();
        container.Resolve<IDecoratedService>();

        Assert.Equal("the-value", capturedRegistration!.Metadata[MetadataKey]);
    }

    [Fact]
    public void MetadataPropertyExposesTheUnderlyingDictionary()
    {
        var builder = new ContainerBuilder();
        var decorator = builder.RegisterDecorator<DecoratorA, IDecoratedService>()
            .WithMetadata(MetadataKey, "first");

        Assert.Equal("first", decorator.Metadata[MetadataKey]);

        // Integrations that need to append to an existing value can go via the dictionary.
        decorator.Metadata[MetadataKey] = "second";

        Assert.Equal("second", decorator.Metadata[MetadataKey]);
    }

    [Fact]
    public void MetadataCanBeProvidedAsASequence()
    {
        var builder = new ContainerBuilder();
        var decorator = builder.RegisterDecorator<DecoratorA, IDecoratedService>()
            .WithMetadata(new Dictionary<string, object?> { [MetadataKey] = "the-value" });

        Assert.Equal("the-value", decorator.Metadata[MetadataKey]);
    }

    [Fact]
    public void WithConditionIsAppliedToTheDecorator()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Implementor>().As<IDecoratedService>();
        builder.RegisterDecorator<DecoratorA, IDecoratedService>()
            .WithCondition(_ => false);

        var container = builder.Build();

        Assert.IsType<Implementor>(container.Resolve<IDecoratedService>());
    }

    [Fact]
    public void WithConditionReplacesTheConditionPassedToRegisterDecorator()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Implementor>().As<IDecoratedService>();
        builder.RegisterDecorator<DecoratorA, IDecoratedService>(_ => false)
            .WithCondition(_ => true);

        var container = builder.Build();

        Assert.IsType<DecoratorA>(container.Resolve<IDecoratedService>());
    }

    [Fact]
    public void WithConditionReceivesTheDecoratorContext()
    {
        IDecoratorContext? capturedContext = null;

        var builder = new ContainerBuilder();
        builder.RegisterType<Implementor>().As<IDecoratedService>();
        builder.RegisterDecorator<DecoratorA, IDecoratedService>()
            .WithCondition(context =>
            {
                capturedContext = context;
                return true;
            });

        var container = builder.Build();
        container.Resolve<IDecoratedService>();

        Assert.NotNull(capturedContext);
        Assert.Equal(typeof(IDecoratedService), capturedContext.ServiceType);
        Assert.Equal(typeof(Implementor), capturedContext.ImplementationType);
    }

    [Fact]
    public void TypedOverloadReturnsAConfigurableBuilder()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Implementor>().As<IDecoratedService>();
        builder.RegisterDecorator(typeof(DecoratorA), typeof(IDecoratedService))
            .ConfigurePipeline(UseWrapper);

        var container = builder.Build();

        Assert.Equal("Wrapper(A(Implementor))", container.Resolve<IDecoratedService>().Name);
    }

    [Fact]
    public void LambdaOverloadReturnsAConfigurableBuilder()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Implementor>().As<IDecoratedService>();
        builder.RegisterDecorator<IDecoratedService>((_, _, inner) => new DecoratorA(inner))
            .ConfigurePipeline(UseWrapper);

        var container = builder.Build();

        Assert.Equal("Wrapper(A(Implementor))", container.Resolve<IDecoratedService>().Name);
    }

    [Fact]
    public void ConfigurationIsAppliedWhenTheContainerIsBuilt()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Implementor>().As<IDecoratedService>();
        var decorator = builder.RegisterDecorator<DecoratorA, IDecoratedService>();

        // Deliberately configured well after the RegisterDecorator call, and after other
        // registrations have been made.
        builder.RegisterType<Implementor>();
        decorator.ConfigurePipeline(UseWrapper);

        var container = builder.Build();

        Assert.Equal("Wrapper(A(Implementor))", container.Resolve<IDecoratedService>().Name);
    }

    [Fact]
    public void ConfigurePipelineRequiresAnAction()
    {
        var builder = new ContainerBuilder();
        var decorator = builder.RegisterDecorator<DecoratorA, IDecoratedService>();

        Assert.Throws<ArgumentNullException>(() => decorator.ConfigurePipeline(null!));
    }

    [Fact]
    public void WithMetadataRequiresAKey()
    {
        var builder = new ContainerBuilder();
        var decorator = builder.RegisterDecorator<DecoratorA, IDecoratedService>();

        Assert.Throws<ArgumentNullException>(() => decorator.WithMetadata(null!, "value"));
    }

    [Fact]
    public void WithMetadataRequiresProperties()
    {
        var builder = new ContainerBuilder();
        var decorator = builder.RegisterDecorator<DecoratorA, IDecoratedService>();

        Assert.Throws<ArgumentNullException>(() => decorator.WithMetadata(null!));
    }

    [Fact]
    public void WithConditionRequiresACondition()
    {
        var builder = new ContainerBuilder();
        var decorator = builder.RegisterDecorator<DecoratorA, IDecoratedService>();

        Assert.Throws<ArgumentNullException>(() => decorator.WithCondition(null!));
    }

    private class Implementor : IDecoratedService
    {
        public string Name => "Implementor";
    }

    private abstract class Decorator : IDecoratedService
    {
        protected Decorator(IDecoratedService decorated)
        {
            Decorated = decorated;
        }

        public IDecoratedService Decorated
        {
            get;
        }

        public abstract string Name
        {
            get;
        }
    }

    private class DecoratorA : Decorator
    {
        public DecoratorA(IDecoratedService decorated)
            : base(decorated)
        {
        }

        public override string Name => $"A({Decorated.Name})";
    }

    private class DecoratorB : Decorator
    {
        public DecoratorB(IDecoratedService decorated)
            : base(decorated)
        {
        }

        public override string Name => $"B({Decorated.Name})";
    }

    /// <summary>
    /// Stands in for the proxy an interception integration would create around the activated
    /// instance.
    /// </summary>
    private class Wrapper : IDecoratedService
    {
        private readonly IDecoratedService _wrapped;

        public Wrapper(IDecoratedService wrapped)
        {
            _wrapped = wrapped;
        }

        public string Name => $"Wrapper({_wrapped.Name})";
    }

    /// <summary>
    /// Adds middleware that replaces the activated instance with a wrapper, in the same place in
    /// the pipeline that an interception integration would use.
    /// </summary>
    /// <param name="pipeline">The pipeline builder.</param>
    private static void UseWrapper(IResolvePipelineBuilder pipeline)
    {
        pipeline.Use(PipelinePhase.Activation, MiddlewareInsertionMode.StartOfPhase, (ctx, next) =>
        {
            next(ctx);

            ctx.Instance = new Wrapper((IDecoratedService)ctx.Instance!);
        });
    }
}
