using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Autofac.Extras.Attributed;

namespace Autofac.Extras.Tests.Attributed.ScenarioTypes
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class ProvidedMetadataAttribute : Attribute, IMetadataProvider
    {
        public IDictionary<string, object> GetMetadata(Type targetType)
        {
            return new Dictionary<string, object>()
            {
                { "Key1", "Value1" },
                { "Key2", "Value2" }
            };
        }
    }

    public class ProvidedMetadata
    {
        public string Key1 { get; set; }
        public string Key2 { get; set; }
    }

    public interface IMetadataProviderScenario
    {
    }

    [ProvidedMetadata]
    public class MetadataProviderScenario : IMetadataProviderScenario { }
}
