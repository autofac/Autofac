
namespace AutofacContrib.Tests.Attributed.ScenarioTypes
{
    [ExportScenario2Metadata(Name = "Hello2")]
    public class ExportScenario2 : IExportScenario2 { }

    [ExportScenario2Metadata(Name="scenario2")]
    public class ExportScenario2Alternate : IExportScenario2 {}
}
