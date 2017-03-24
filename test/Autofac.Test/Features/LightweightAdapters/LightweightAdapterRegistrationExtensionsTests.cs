using System.Collections.Generic;
using System.Linq;
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
                    builder.RegisterInstance(command);
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

        public class DecoratingServiceThatHasDefaultImplementation
        {
            private readonly IContainer _container;

            public DecoratingServiceThatHasDefaultImplementation()
            {
                const string from = "from";
                var builder = new ContainerBuilder();

                builder.RegisterType<Implementer1>().As<IService>().Named<IService>(from);
                builder.RegisterDecorator<IService>(s => new Decorator(s), from);

                _container = builder.Build();
            }

            [Fact(Skip = "Issue #529")]
            public void InstanceWithDefaultImplementationIsDecorated()
            {
                var decorator = _container.Resolve<IService>();
                Assert.IsType<Decorator>(decorator);
                Assert.IsType<Implementer1>(((Decorator)decorator).Decorated);
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
            private readonly IService _decorated;

            public Decorator(IService decorated)
            {
                _decorated = decorated;
            }

            public IService Decorated
            {
                get { return _decorated; }
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
                Assert.True(all.Any(i => i is Implementer1));
                Assert.True(all.Any(i => i is Implementer2));
            }
        }
    }
}
