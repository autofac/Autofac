using System;
using AutofacContrib.Attributed.MEF;
using NUnit.Framework;

namespace AutofacContrib.Tests.Attributed
{
    [TestFixture]
    public class ExtendedExportInfoFixture
    {
        #region local test types

        public class TestInterface
        { }

        public class TestInterfaceMetadata
        { }

        #endregion

        [Test]
        public void verify_information_passthru_on_instantiation()
        {
            var item = new ExtendedExportInfo<TestInterface, TestInterfaceMetadata>
                                (typeof(TestInterface), new Lazy<TestInterface, TestInterfaceMetadata>(() => new TestInterface(),
                                                                                                       new TestInterfaceMetadata()));

            Assert.That(item.InstantiationType.AssemblyQualifiedName, Is.EqualTo(typeof(TestInterface).AssemblyQualifiedName));
            Assert.That(item.Value.Value, Is.TypeOf<TestInterface>());
            Assert.That(item.Value.Metadata, Is.TypeOf<TestInterfaceMetadata>());
        }

        [Test]
        public void test_for_exception_handling_of_null_type_on_ctor()
        {
            var assertDetails =
                Assert.Throws<ArgumentNullException>(
                    () => new ExtendedExportInfo<TestInterface, TestInterfaceMetadata>(null, null));

            Assert.That(assertDetails.ParamName, Is.EqualTo("instantiationType"));
        }

        [Test]
        public void test_for_exception_handling_of_null_lazy_instance_on_ctor()
        {
            var assertDetails =
                Assert.Throws<ArgumentNullException>(
                    () => new ExtendedExportInfo<TestInterface, TestInterfaceMetadata>(typeof(TestInterface), null));

            Assert.That(assertDetails.ParamName, Is.EqualTo("value"));
        }
    }
}
