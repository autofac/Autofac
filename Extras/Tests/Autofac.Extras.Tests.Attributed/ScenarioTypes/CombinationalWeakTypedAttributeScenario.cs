using System;
using System.ComponentModel.Composition;

namespace Autofac.Extras.Tests.Attributed.ScenarioTypes
{
    #region interfaces

    public interface ICombinationalWeakTypedScenarioMetadata
    {
        string Name { get; }
        int Age { get; }
    }

    public interface ICombinationalWeakTypedScenario
    { }

    #endregion

    #region attribute

    [MetadataAttribute]
    public class CombinationalWeakNameMetadataAttribute : Attribute
    {
        public string Name { get; private set; }

        public CombinationalWeakNameMetadataAttribute(string name)
        {
            Name = name;
        }
    }


    [MetadataAttribute]
    public class CombinationalWeakAgeMetadataAttribute : Attribute
    {
        public int Age { get; private set; }

        public CombinationalWeakAgeMetadataAttribute(int age)
        {
            Age = age;
        }
    }


    #endregion

    [CombinationalWeakNameMetadata("Hello")]
    [CombinationalWeakAgeMetadata(42)]
    public class CombinationalWeakTypedScenario : ICombinationalWeakTypedScenario { }

}
