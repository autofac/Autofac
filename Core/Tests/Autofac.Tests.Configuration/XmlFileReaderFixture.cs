using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Configuration;
using NUnit.Framework;

namespace Autofac.Tests.Configuration
{
    [TestFixture]
    public class XmlFileReaderFixture
    {
        /* The tests for the various configuration options are in the ConfigurationSettingsReaderFixture.
         * As the configuration mechanism all goes through a single base class, there's no value
         * in re-testing the same parsing mechanism here. */

        [Test(Description = "Attempts to initialize the module with an empty file name.")]
        public void Ctor_EmptyFileName()
        {
            Assert.Throws<ArgumentException>(() => new XmlFileReader(""));
        }

        [Test(Description = "Attempts to initialize the module without providing a file name.")]
        public void Ctor_NullFileName()
        {
            Assert.Throws<ArgumentNullException>(() => new XmlFileReader(null));
        }

        [Test(Description = "Registers components using a regular XML file.")]
        public void Load_ProcessXml()
        {
            var cb = new ContainerBuilder();
            var module = new XmlFileReader("Files/RefereeXml.config");
            cb.RegisterModule(module);
            var container = cb.Build();
            container.AssertRegisteredNamed<object>("c", "The component from the configuration file was not registered.");
        }
    }
}
