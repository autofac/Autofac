using System;
using System.ComponentModel.Composition;

namespace AutofacContrib.Tests.Attributed.ScenarioTypes
{
    [MetadataAttribute]
    public class ExportScenario1MetadataAttribute : Attribute
    {
        public string Name { get; private set; }

        public ExportScenario1MetadataAttribute(string name)
        {
            Name = name;
        }
    }
}
