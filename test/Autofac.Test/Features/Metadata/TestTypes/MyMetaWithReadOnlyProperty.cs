using System.Collections.Generic;
using System.ComponentModel;

namespace Autofac.Test.Features.Metadata.TestTypes
{
    public class MyMetaWithReadOnlyProperty
    {
        public int TheInt { get; set; }

        public string ReadOnly
        {
            get
            {
                return "Foo";
            }
        }
    }
}
