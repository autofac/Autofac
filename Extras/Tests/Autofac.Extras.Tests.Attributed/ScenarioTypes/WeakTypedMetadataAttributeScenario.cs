using System;
using System.ComponentModel.Composition;

namespace AutofacContrib.Tests.Attributed.ScenarioTypes
{
    #region interfaces

    public interface IWeakTypedScenarioMetadata
    {
        string Name { get; }
    }

    public interface IWeakTypedScenario
    { }

    #endregion

    #region attribute

    [MetadataAttribute]
    public class WeakTypedScenarioMetadataAttribute : Attribute
    {
        public string Name { get; private set; }

        public WeakTypedScenarioMetadataAttribute(string name)
        {
            Name = name;
        }
    }

    #endregion

    [WeakTypedScenarioMetadata("Hello")]
    public class WeakTypedScenario : IWeakTypedScenario { }
}