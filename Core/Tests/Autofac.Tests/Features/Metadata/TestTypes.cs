using System.ComponentModel;

namespace Autofac.Tests.Features.Metadata
{
    public class MyMeta
    {
        public int TheInt { get; set; }
    }

    public class MyMetaWithDefault
    {
        [DefaultValue(42)]
        public int TheInt { get; set; }
    }
}
