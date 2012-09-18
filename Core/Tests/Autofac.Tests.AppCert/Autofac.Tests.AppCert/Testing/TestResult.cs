using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Autofac.Tests.AppCert.Testing
{
    public class TestResult
    {
        public bool Success { get; set; }
        public MethodInfo TestMethod { get; set; }
        public string Message { get; set; }
    }
}
