using Autofac.Core;
using Xunit;
using System.IO;

namespace Autofac.Test.Core
{
    public class DependencyResolutionExceptionTests
    {
        public DependencyResolutionExceptionTests()
        {
            //Explicitly set culture for comparison of Exception strings
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
        }

        [Fact]
        public void Message_InnerExceptionMessageIncluded()
        {
            // Issue 343: The inner exception message should be included in the main exception message.
            var inner = new FileNotFoundException("Can't find file.");
            var dre = new DependencyResolutionException("Unable to resolve component.", inner);
            Assert.True(dre.Message.Contains("Can't find file."), "The exception message should include the inner exception message.");
        }

        [Fact]
        public void Message_NoInnerException()
        {
            // Issue 343: If there is no inner exception specified, the main exception message should not be modified.
            var dre = new DependencyResolutionException("Unable to resolve component.");
            Assert.Equal("Unable to resolve component.", dre.Message);
        }

        [Fact]
        public void Message_NoMessageOrInnerException()
        {
            // Issue 343: If there is no message or inner exception specified, the main exception message should not be modified.
            var dre = new DependencyResolutionException(null);
            Assert.Equal("Exception of type 'Autofac.Core.DependencyResolutionException' was thrown.", dre.Message);
        }

        [Fact]
        public void Message_NullInnerException()
        {
            // Issue 343: If there is a null inner exception specified, the main exception message should not be modified.
            var dre = new DependencyResolutionException("Unable to resolve component.", null);
            Assert.Equal("Unable to resolve component.", dre.Message);
        }

        [Fact]
        public void Message_NullMessageWithInnerException()
        {
            // Issue 343: If there is no message but there is an inner exception specified, the main exception message should be modified.
            var inner = new FileNotFoundException("Can't find file.");
            var dre = new DependencyResolutionException(null, inner);
            Assert.True(dre.Message.Contains("Can't find file."), "The exception message should include the inner exception message.");
        }
    }
}
