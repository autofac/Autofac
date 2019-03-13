using System;
using System.Collections.Generic;
using System.Text;

namespace Autofac.Specification.Test.Features.PropertyInjection
{
    public class HasMixedVisibilityProperties
    {
        public string PublicString { get; set; }

        [Inject]
        private string PrivateString { get; set; }

        public string PrivateStringAccessor()
        {
            return this.PrivateString;
        }
    }
}
