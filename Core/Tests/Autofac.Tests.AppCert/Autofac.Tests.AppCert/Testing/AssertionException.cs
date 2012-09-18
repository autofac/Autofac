using System;
using System.Collections.Generic;
using System.Linq;

namespace Autofac.Tests.AppCert.Testing
{
    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message) { }
        public AssertionException(string message, Exception inner) : base(message, inner) { }
    }
}
