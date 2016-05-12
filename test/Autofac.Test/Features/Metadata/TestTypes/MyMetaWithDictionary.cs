using System.Collections.Generic;
using System.ComponentModel;

namespace Autofac.Test.Features.Metadata.TestTypes
{
    public class MyMetaWithDictionary
    {
        public MyMetaWithDictionary(IDictionary<string, object> metadata)
        {
            TheName = (string)metadata["Name"];
        }

        public string TheName { get; set; }
    }
}
