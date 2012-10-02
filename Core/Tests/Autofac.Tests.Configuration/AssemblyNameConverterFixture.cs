using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Configuration;
using NUnit.Framework;

namespace Autofac.Tests.Configuration
{
    [TestFixture]
    public class AssemblyNameConverterFixture
    {
        [Test(Description = "Attempts to convert empty/null assembly name values to an assembly.")]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void ConvertFrom_AssemblyNameEmpty(string value)
        {
            var converter = new AssemblyNameConverter();
            Assert.IsNull(converter.ConvertFrom(value), "Empty/null assembly name should result in a null converted value.");
        }

        [Test(Description = "Converts a full assembly name to an assembly.")]
        public void ConvertFrom_FullAssemblyName()
        {
            var converter = new AssemblyNameConverter();
            var expected = typeof(String).Assembly;
            var actual = converter.ConvertFrom(expected.FullName);
            Assert.AreEqual(expected, actual, "The assembly loaded was not the one expected.");
        }

        [Test(Description = "Converts a simple assembly name (no version) to an assembly.")]
        public void ConvertFrom_SimpleAssemblyName()
        {
            var converter = new AssemblyNameConverter();
            var expected = typeof(String).Assembly;
            var actual = converter.ConvertFrom("mscorlib");
            Assert.AreEqual(expected, actual, "The assembly loaded was not the one expected.");
        }

        [Test(Description = "Attempts to convert a value that isn't an assembly into a string.")]
        public void ConvertTo_NotAssembly()
        {
            var converter = new AssemblyNameConverter();
            Assert.Throws<ArgumentException>(() => converter.ConvertTo(5, null));
        }

        [Test(Description = "Converts a null assembly value to a string.")]
        public void ConvertTo_NullAssembly()
        {
            var converter = new AssemblyNameConverter();
            Assert.IsNull(converter.ConvertTo(null, null), "A null value should convert to a null assembly.");
        }

        [Test(Description = "Converts an assembly value to a string.")]
        public void ConvertTo_ValidAssembly()
        {
            var converter = new AssemblyNameConverter();
            var asm = typeof(String).Assembly;
            Assert.AreEqual(asm.FullName, converter.ConvertTo(asm, null), "An assembly value should convert to the full assembly name.");
        }
    }
}
