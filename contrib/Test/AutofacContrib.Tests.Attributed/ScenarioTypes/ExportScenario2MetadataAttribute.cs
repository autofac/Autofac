using System;
using System.ComponentModel.Composition;

namespace AutofacContrib.Tests.Attributed.ScenarioTypes
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ExportScenario2MetadataAttribute : Attribute, IExportScenario2Metadata
    {

        #region IFooMetadata Members

        public string Name { get; set; }

        #endregion
    }
}
