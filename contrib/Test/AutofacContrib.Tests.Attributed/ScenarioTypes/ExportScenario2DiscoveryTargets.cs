
namespace AutofacContrib.Tests.Attributed.ScenarioTypes
{
    [ExportScenario2Metadata(typeof(IExportScenario2), Name = "Hello2")]
    public class ExportScenario2 : IExportScenario2 { }

    [ExportScenario2Metadata(typeof(IExportScenario2), Name="scenario2")]
    public class ExportScenario2Alternate : IExportScenario2 {}
}
