using System;
using System.Collections.Generic;
using System.Linq;

namespace Autofac.Tests.AppCert.Testing
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false, Inherited=true)]
    public class TestAttribute : Attribute
    {
    }
}
