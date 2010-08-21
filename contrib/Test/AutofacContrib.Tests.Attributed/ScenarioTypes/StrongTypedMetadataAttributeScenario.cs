using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace AutofacContrib.Tests.Attributed.ScenarioTypes
{
    #region interfaces

    public interface IStrongTypedScenarioMetadata
    {
        string Name { get; }
        int Age { get; }
    }

    public interface IStrongTypedScenario
    { }

    #endregion

    #region attribute


    [MetadataAttribute]
    public class StrongTypedScenarioMetadataAttribute : Attribute, IStrongTypedScenarioMetadata
    {
        

        public StrongTypedScenarioMetadataAttribute(string name, int age)
        {
            Name = name;
            Age = age;
        }
    
        #region IStrongTypedScenarioMetadata Members
        public string Name { get; private set; }

        public int  Age { get; private set; }
        #endregion
    }
        


    #endregion

    [StrongTypedScenarioMetadata("Hello", 42)]
    public class StrongTypedScenario : IStrongTypedScenario { }

    [StrongTypedScenarioMetadata("Goodbye", 24)]
    public class AlternateStrongTypedScenario : IStrongTypedScenario {}
}
