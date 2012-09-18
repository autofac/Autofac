using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Autofac.Tests.AppCert.Testing
{
    public class TestRunner
    {
        public IEnumerable<Type> TestFixtures { get; private set; }

        public TestRunner(string testNamespace)
        {
            if (testNamespace == null)
            {
                throw new ArgumentNullException("testNamespace");
            }
            if (testNamespace.Length == 0)
            {
                throw new ArgumentException("Test fixture namespace may not be empty.", "testNamespace");
            }
            this.LocateTestFixtures(testNamespace);
        }

        private void LocateTestFixtures(string testNamespace)
        {
            this.TestFixtures = typeof(TestRunner).GetTypeInfo().Assembly.DefinedTypes.Where(t => t.GetCustomAttribute<TestFixtureAttribute>(true) != null && t.FullName.StartsWith(testNamespace + ".")).Select(t => t.AsType());
        }

        public IEnumerable<TestResult> ExecuteTests()
        {
            foreach (var fixtureType in this.TestFixtures)
            {
                var fixture = Activator.CreateInstance(fixtureType);
                foreach (var testMethod in fixtureType.GetRuntimeMethods().Where(m => m.GetCustomAttribute<TestAttribute>() != null))
                {
                    var result = new TestResult
                    {
                        TestMethod = testMethod
                    };
                    try
                    {
                        testMethod.Invoke(fixture, null);
                        result.Success = true;
                    }
                    catch (AssertionException ex)
                    {
                        result.Success = false;
                        result.Message = ex.Message;
                    }
                    catch (Exception ex)
                    {
                        result.Success = false;
                        result.Message = string.Format("Unexpected exception: {0}", ex);
                    }
                    yield return result;
                }
            }
        }
    }
}
