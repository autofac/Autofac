using System.ComponentModel.Composition;

namespace AutofacContrib.Tests.Attributed.ScenarioTypes
{
    [ExportScenario1Metadata("Hello")]
    [Export(typeof(IExportScenario1))]
    public class ExportScenario1 : IExportScenario1 { }
}
