using System;
using System.ComponentModel.Composition;

namespace AutofacContrib.Tests.Attributed.ScenarioTypes
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ExportScenario3MetadataAttribute : ExportAttribute, IExportScenario3Metadata
    {
        public ExportScenario3MetadataAttribute(string name) : base(typeof(IExportScenario3Metadata))
        {
            Name = name;
        }

        #region IFooMetadata Members

        public string Name { get; private set; }

        #endregion
    }
}
