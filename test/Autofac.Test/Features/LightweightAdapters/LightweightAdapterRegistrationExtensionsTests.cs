// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Features.Indexed;
using Autofac.Features.Metadata;
using Autofac.Test.Scenarios.Adapters;
using Xunit;

namespace Autofac.Test.Features.LightweightAdapters
{
    public class LightweightAdapterRegistrationExtensionsTests
    {
        public class AdaptingTypeToType
        {
            private readonly IEnumerable<Command> _commands = new[]
            {
                new Command(),
                new Command(),
            };

            private readonly IEnumerable<IToolbarButton> _toolbarButtons;

            public AdaptingTypeToType()
            {
                var builder = new ContainerBuilder();
                foreach (var command in _commands)
                {
                    builder.RegisterInstance(command);
                }

                builder.RegisterAdapter<Command, ToolbarButton>(cmd => new ToolbarButton(cmd))
                    .As<IToolbarButton>();
                var container = builder.Build();
                _toolbarButtons = container.Resolve<IEnumerable<IToolbarButton>>();
            }

            [Fact]
            public void AdaptingTypeSeesKeysOfAdapteeType()
            {
                var builder = new ContainerBuilder();

                builder.RegisterType<Command>().Keyed<Command>("Command");
                builder.RegisterType<AnotherCommand>().Keyed<AnotherCommand>("AnotherCommand");
                builder.RegisterAdapter<Command, AnotherCommand>(c => new AnotherCommand());

                var container = builder.Build();

                var command = container.Resolve<IIndex<string, AnotherCommand>>()["Command"];
                Assert.NotNull(command);
                Assert.IsType<AnotherCommand>(command);

                var anotherCommand = container.Resolve<IIndex<string, AnotherCommand>>()["AnotherCommand"];
                Assert.NotNull(anotherCommand);
                Assert.IsType<AnotherCommand>(anotherCommand);
            }

            [Fact]
            public void EachInstanceOfTheTargetTypeIsAdapted()
            {
                Assert.True(_commands.All(cmd => _toolbarButtons.Any(b => b.Command == cmd)));
            }
        }

        public class OnTopOfAnotherAdapter
        {
            private readonly Command _from = new Command();
            private const string NameKey = "Name";
            private const string Name = "N";
            private readonly ToolbarButton _to;

            public OnTopOfAnotherAdapter()
            {
                var builder = new ContainerBuilder();
                builder.RegisterInstance(_from).WithMetadata(NameKey, Name);
                builder.RegisterAdapter<Meta<Command>, ToolbarButton>(
                    cmd => new ToolbarButton(cmd.Value, (string)cmd.Metadata[NameKey]));
                var container = builder.Build();
                _to = container.Resolve<ToolbarButton>();
            }

            [Fact]
            public void AdaptedMetadataIsPassed()
            {
                Assert.Equal(Name, _to.Name);
            }
        }

        public interface IService
        {
        }

        public class Implementer1 : IService
        {
        }

        public class Implementer2 : IService
        {
        }

        public class Decorator : IService
        {
            public Decorator(IService decorated)
            {
                Decorated = decorated;
            }

            public IService Decorated { get; }
        }

        public class DecoratorParameterization
        {
            private readonly IContainer _container;

            public DecoratorParameterization()
            {
                var builder = new ContainerBuilder();
                builder.Register((ctx, p) => new ParameterizedImplementer(p)).Named<IParameterizedService>("from");
                builder.RegisterDecorator<IParameterizedService>((ctx, p, s) => new ParameterizedDecorator1(s, p), "from", "to");
                builder.RegisterDecorator<IParameterizedService>((ctx, p, s) => new ParameterizedDecorator2(s, p), "to");
                _container = builder.Build();
            }

            [Fact]
            public void ParametersGoToTheDecoratedInstance()
            {
                var resolved = _container.Resolve<IParameterizedService>(TypedParameter.From<IService>(new Implementer1()));
                var dec2 = Assert.IsType<ParameterizedDecorator2>(resolved);
                Assert.Empty(dec2.Parameters);
                var dec1 = Assert.IsType<ParameterizedDecorator1>(dec2.Implementer);
                Assert.Empty(dec1.Parameters);
                var imp = Assert.IsType<ParameterizedImplementer>(dec1.Implementer);
                Assert.Single(imp.Parameters);
            }

            public interface IParameterizedService
            {
                IEnumerable<Parameter> Parameters { get; }
            }

            public class ParameterizedImplementer : IParameterizedService
            {
                public ParameterizedImplementer(IEnumerable<Parameter> parameters)
                {
                    Parameters = parameters;
                }

                public IEnumerable<Parameter> Parameters { get; }
            }

            public class ParameterizedDecorator1 : IParameterizedService
            {
                public ParameterizedDecorator1(IParameterizedService implementer, IEnumerable<Parameter> parameters)
                {
                    Implementer = implementer;
                    Parameters = parameters;
                }

                public IParameterizedService Implementer { get; }

                public IEnumerable<Parameter> Parameters { get; }
            }

            public class ParameterizedDecorator2 : IParameterizedService
            {
                public ParameterizedDecorator2(IParameterizedService implementer, IEnumerable<Parameter> parameters)
                {
                    Implementer = implementer;
                    Parameters = parameters;
                }

                public IParameterizedService Implementer { get; }

                public IEnumerable<Parameter> Parameters { get; }
            }
        }

        public class DecoratingANamedService
        {
            private readonly IContainer _container;

            public DecoratingANamedService()
            {
                const string from = "from";
                var builder = new ContainerBuilder();
                builder.RegisterType<Implementer1>().Named<IService>(from);
                builder.RegisterType<Implementer2>().Named<IService>(from);
                builder.RegisterDecorator<IService>(s => new Decorator(s), from);
                _container = builder.Build();
            }

            [Fact]
            public void TheDefaultNamedInstanceIsTheDefaultDecoratedInstance()
            {
                var d = _container.Resolve<IService>();
                Assert.IsType<Implementer2>(((Decorator)d).Decorated);
            }

            [Fact]
            public void AllInstancesAreDecorated()
            {
                var all = _container.Resolve<IEnumerable<IService>>()
                    .Cast<Decorator>()
                    .Select(d => d.Decorated);

                Assert.Equal(2, all.Count());
                Assert.Contains(all, i => i is Implementer1);
                Assert.Contains(all, i => i is Implementer2);
            }
        }
    }
}
