using System.ComponentModel;

namespace Autofac.Tests.Integration.Mef
{
    public interface IMeta
    {
        int TheInt { get; }
    }

    public interface IMetaWithDefault
    {
        [DefaultValue(42)]
        int TheInt { get; }
    }
}
