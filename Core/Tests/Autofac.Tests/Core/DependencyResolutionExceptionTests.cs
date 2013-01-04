using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac.Core;
using NUnit.Framework;

namespace Autofac.Tests.Core
{
    [TestFixture]
    public class DependencyResolutionExceptionTests
    {
        [Test(Description = "Issue 343: The inner exception message should be included in the main exception message.")]
        public void Message_InnerExceptionMessageIncluded()
        {
            var inner = new FileNotFoundException("Can't find file.");
            var dre = new DependencyResolutionException("Unable to resolve component.", inner);
            Assert.IsTrue(dre.Message.Contains("Can't find file."), "The exception message should include the inner exception message.");
        }

        [Test(Description = "Issue 343: If there is no inner exception specified, the main exception message should not be modified.")]
        public void Message_NoInnerException()
        {
            var dre = new DependencyResolutionException("Unable to resolve component.");
            Assert.AreEqual("Unable to resolve component.", dre.Message, "The message should not be modified if there is no inner exception.");
        }

        [Test(Description = "Issue 343: If there is no message or inner exception specified, the main exception message should not be modified.")]
        public void Message_NoMessageOrInnerException()
        {
            var dre = new DependencyResolutionException(null);
            Assert.AreEqual("Exception of type 'Autofac.Core.DependencyResolutionException' was thrown.", dre.Message, "The message should not be modified if there is no inner exception.");
        }

        [Test(Description = "Issue 343: If there is a null inner exception specified, the main exception message should not be modified.")]
        public void Message_NullInnerException()
        {
            var dre = new DependencyResolutionException("Unable to resolve component.", null);
            Assert.AreEqual("Unable to resolve component.", dre.Message, "The message should not be modified if there is no inner exception.");
        }

        [Test(Description = "Issue 343: If there is no message but there is an inner exception specified, the main exception message should be modified.")]
        public void Message_NullMessageWithInnerException()
        {
            var inner = new FileNotFoundException("Can't find file.");
            var dre = new DependencyResolutionException(null, inner);
            Assert.IsTrue(dre.Message.Contains("Can't find file."), "The exception message should include the inner exception message.");
        }
    }
}
