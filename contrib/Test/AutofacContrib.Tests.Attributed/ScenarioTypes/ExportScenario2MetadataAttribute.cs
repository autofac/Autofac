using System;
using System.ComponentModel.Composition;

namespace AutofacContrib.Tests.Attributed.ScenarioTypes
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ExportScenario2MetadataAttribute : ExportAttribute, IExportScenario2Metadata
    {
        public ExportScenario2MetadataAttribute() { }
        public ExportScenario2MetadataAttribute(Type contractType) : base(contractType) { }
        public ExportScenario2MetadataAttribute(string contractName) : base(contractName) { }
        public ExportScenario2MetadataAttribute(string contractName, Type contractType) : base(contractName, contractType) { }

        #region IFooMetadata Members

        public string Name { get; set; }

        #endregion
    }
}
