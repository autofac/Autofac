using System.Collections.Generic;
using System.ComponentModel;

namespace Autofac.Test.Features.Metadata.TestTypes
{
    public class MyMetaWithInvalidConstructor
    {
        public MyMetaWithInvalidConstructor(string unsupported)
        {
        }
    }
}
