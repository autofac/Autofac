using System.Collections.Generic;
using System.ComponentModel;

namespace Autofac.Test.Features.Metadata.TestTypes
{
    public class MyMetaWithDefault
    {
        [DefaultValue(42)]
        public int TheInt { get; set; }
    }
}
