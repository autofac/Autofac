using System.Net;
using Autofac.Configuration.Util;
using NUnit.Framework;

namespace Autofac.Tests.Configuration
{
    [TestFixture]
    public class TypeManipulationFixture
    {
        [Test]
        public void ChangeToCompatibleTypeLooksForTryParseMethod()
        {
            const string address = "127.0.0.1";
            var value = TypeManipulation.ChangeToCompatibleType(address, typeof(IPAddress));
            Assert.That(value, Is.EqualTo(IPAddress.Parse(address)));
        }
    }
}