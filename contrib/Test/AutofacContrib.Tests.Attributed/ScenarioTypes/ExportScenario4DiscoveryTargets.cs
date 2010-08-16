
namespace AutofacContrib.Tests.Attributed.ScenarioTypes
{
    public class ExportScenario4 : IExportScenario4
    {
    }

    public class ExportScenario4Alternate : IExportScenario4
    {
    }

    public class ExportScenario4Metadata : IExportScenario4Metadata
    {
        public ExportScenario4Metadata(string name)
        {
            Name = name;
        }

        #region IExportScenario4Metadata Members

        public string Name { get; private set;} 

        #endregion
    }

}
