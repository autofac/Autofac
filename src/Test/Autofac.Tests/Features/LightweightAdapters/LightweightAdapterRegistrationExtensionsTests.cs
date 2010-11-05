using System.Collections.Generic;
using System.Linq;
using Autofac.Features.Metadata;
using Autofac.Tests.Scenarios.Adapters;
using NUnit.Framework;

namespace Autofac.Tests.Features.LightweightAdapters
{
    public class LightweightAdapterRegistrationExtensionsTests
    {
        [TestFixture]
        public class AdaptingTypeToType
        {
            readonly IEnumerable<Command> _commands = new[]
            {
                new Command(),
                new Command()
            };

            readonly IEnumerable<IToolbarButton> _toolbarButtons;

            public AdaptingTypeToType()
            {
                var builder = new ContainerBuilder();
                foreach (var command in _commands)
                    builder.RegisterInstance(command);
#if !(SL2 || SL3 || NET35)
                builder.RegisterAdapter<Command, ToolbarButton>(cmd => new ToolbarButton(cmd)) 
#else
                builder.RegisterAdapter<Command, ToolbarButton>(cmd => new ToolbarButton(cmd, "")) 
#endif
                    .As<IToolbarButton>();
                var container = builder.Build();
                _toolbarButtons = container.Resolve<IEnumerable<IToolbarButton>>();
            }

            [Test]
            public void EachInstanceOfTheTargetTypeIsAdapted()
            {
                Assert.That(_commands.All(cmd => _toolbarButtons.Any(b => b.Command == cmd)));
            }
        }

        [TestFixture]
        public class OnTopOfAnotherAdapter
        {
            readonly Command _from = new Command();
            readonly string _nameKey = "Name";
            readonly string _name = "N";
            readonly ToolbarButton _to;

            public OnTopOfAnotherAdapter()
            {
                var builder = new ContainerBuilder();
                builder.RegisterInstance(_from).WithMetadata(_nameKey, _name);
                builder.RegisterAdapter<Meta<Command>, ToolbarButton>(
                    cmd => new ToolbarButton(cmd.Value, (string)cmd.Metadata[_nameKey]));
                var container = builder.Build();
                _to = container.Resolve<ToolbarButton>();
            }

            [Test]
            public void AdaptedMetadataIsPassed()
            {
                Assert.AreEqual(_name, _to.Name);
            }
        }

        interface IService { }

        class Implementer1 : IService { }

        class Implementer2 : IService { }

        class Decorator : IService
        {
            readonly IService _decorated;

            public Decorator(IService decorated)
            {
                _decorated = decorated;
            }

            public IService Decorated
            {
                get { return _decorated; }
            }
        }

        [TestFixture]
        public class DecoratingANamedService
        {
            readonly IContainer _container;

            public DecoratingANamedService()
            {
                const string from = "from";
                var builder = new ContainerBuilder();
                builder.RegisterType<Implementer1>().Named<IService>(from);
                builder.RegisterType<Implementer2>().Named<IService>(from);
                builder.RegisterDecorator<IService>(s => new Decorator(s), from);
                _container = builder.Build();
            }

            [Test]
            public void TheDefaultNamedInstanceIsTheDefaultDecoratedInstance()
            {
                var d = _container.Resolve<IService>();
                Assert.IsInstanceOf<Implementer2>(((Decorator)d).Decorated);
            }

            [Test]
            public void AllInstancesAreDecorated()
            {
                var all = _container.Resolve<IEnumerable<IService>>()
                    .Cast<Decorator>()
                    .Select(d => d.Decorated);

                Assert.AreEqual(2, all.Count());
                Assert.That(all.Any(i => i is Implementer1));
                Assert.That(all.Any(i => i is Implementer2));
            }
        }
    }
}
