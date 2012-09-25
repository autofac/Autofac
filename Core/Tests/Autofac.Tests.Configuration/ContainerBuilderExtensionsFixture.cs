using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Autofac.Configuration;
using NUnit.Framework;

namespace Autofac.Tests.Configuration
{
    [TestFixture]
    public class ContainerBuilderExtensionsFixture
    {
        [Test(Description = "Attempts to register configuration with a null container builder.")]
        public void RegisterConfigurationSection_NullBuilder()
        {
            ContainerBuilder builder = null;
            var section = CreateSection();
            Assert.Throws<ArgumentNullException>(() => builder.RegisterConfigurationSection(section));
        }

        [Test(Description = "Attempts to register configuration with a null configuration section.")]
        public void RegisterConfigurationSection_NullSection()
        {
            var builder = new ContainerBuilder();
            Assert.Throws<ArgumentNullException>(() => builder.RegisterConfigurationSection(null));
        }

        private static SectionHandler CreateSection()
        {
            const string metadataConfiguration =
@"<autofac defaultAssembly=""mscorlib"">
  <components>
    <component type=""System.Object"" service=""System.Object"" name=""a"">
      <metadata>
        <item name=""answer"" value=""42"" type=""System.Int32"" />
      </metadata>
    </component>
  </components>
</autofac>";
            using (var reader = new XmlTextReader(new StringReader(metadataConfiguration)))
            {
                return SectionHandler.Deserialize(reader);
            }
        }
    }
}
