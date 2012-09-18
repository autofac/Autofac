using System;
using System.Collections.Generic;
using System.Linq;

namespace Autofac.Tests.AppCert.Testing
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=true)]
    public class TestFixtureAttribute : Attribute
    {
    }
}
