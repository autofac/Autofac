using System.Collections.Generic;
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

    public class MyMetaWithDictionary
    {
        public MyMetaWithDictionary(IDictionary<string, object> metadata)
        {
            TheName = (string)metadata["Name"];
        }

        public string TheName { get; set; }
    }

    public class MyMetaWithReadOnlyProperty
    {
        public int TheInt { get; set; }
        public string ReadOnly { get { return "Foo"; } }
    }

    public class MyMetaWithInvalidConstructor
    {
        public MyMetaWithInvalidConstructor(string unsupported)
        {
        }
    }

    public interface IMyMetaInterface
    {
        int TheInt { get; }
    }
}
