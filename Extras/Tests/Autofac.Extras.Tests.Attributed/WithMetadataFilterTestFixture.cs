using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Autofac.Extras.Attributed;
using NUnit.Framework;

namespace Autofac.Extras.Tests.Attributed
{
    [TestFixture]
    public class WithMetadataFilterTestFixture
    {
        [Test]
        public void verify_metadata_filter_is_applied_on_constructor_dependency_single()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new AttributedMetadataModule());
            builder.RegisterType<ConsoleLogger>().WithMetadata("LoggerName", "Solution").As<ILogger>();
            builder.RegisterType<FileLogger>().WithMetadata("LoggerName", "Other").As<ILogger>();
            builder.RegisterType<SolutionExplorer>().WithMetadataFilter();

            var container = builder.Build();
            var explorer = container.Resolve<SolutionExplorer>();

            Assert.IsInstanceOf<ConsoleLogger>(explorer.Logger, "The logger was not the expected type.");
        }

        [Test]
        public void verify_metadata_filter_is_applied_on_constructor_dependency_multiple()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new AttributedMetadataModule());
            builder.RegisterType<MsBuildAdapter>().As<IAdapter>();
            builder.RegisterType<DteAdapter>().As<IAdapter>();
            builder.RegisterType<ToolWindowAdapter>().As<IAdapter>();
            builder.RegisterType<SolutionExplorer>().WithMetadataFilter();

            var container = builder.Build();

            var allAdapters = container.Resolve<IEnumerable<IAdapter>>().Count();

            Assert.AreEqual(3, allAdapters, "The wrong number of adapters were registered.");

            var explorer = container.Resolve<SolutionExplorer>();

            Assert.AreEqual(2, explorer.Adapters.Count, "The wrong number of adapters were actually filtered into the consuming component.");
        }

        [Test]
        public void verify_components_that_are_not_used_do_not_get_activated()
        {
            int adapterActivationCount = 0;
            var builder = new ContainerBuilder();
            builder.RegisterModule(new AttributedMetadataModule());
            builder.RegisterType<MsBuildAdapter>().As<IAdapter>().OnActivating(h => adapterActivationCount++);
            builder.RegisterType<DteAdapter>().As<IAdapter>().OnActivating(h => adapterActivationCount++);
            builder.RegisterType<ToolWindowAdapter>().As<IAdapter>().OnActivating(h => adapterActivationCount++);
            builder.RegisterType<SolutionExplorer>().WithMetadataFilter();

            var container = builder.Build();
            container.Resolve<SolutionExplorer>();
            Assert.AreEqual(2, adapterActivationCount, "Only the components actually used should be activated.");
        }

        public interface ILogger { }
        public class ConsoleLogger : ILogger { }
        public class FileLogger : ILogger { }

        [MetadataAttribute]
        public class AdapterAttribute : Attribute
        {
            public AdapterAttribute(string target)
            {
                this.Target = target;
            }

            public AdapterAttribute(IDictionary<string, object> metadata)
            {
                this.Target = (string)metadata["Target"];
            }

            public string Target { get; set; }
        }

        public interface IAdapter { }

        [Adapter("Solution")]
        public class MsBuildAdapter : IAdapter { }

        [Adapter("Solution")]
        public class DteAdapter : IAdapter { }

        [Adapter("Other")]
        public class ToolWindowAdapter : IAdapter { }

        public class SolutionExplorer
        {
            public SolutionExplorer(
                [WithMetadata("Target", "Solution")] IEnumerable<IAdapter> adapters,
                [WithMetadata("LoggerName", "Solution")] ILogger logger)
            {
                this.Adapters = adapters.ToList();
                this.Logger = logger;
            }

            public List<IAdapter> Adapters { get; set; }
            public ILogger Logger { get; set; }
        }
    }
}