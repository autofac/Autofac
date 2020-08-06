namespace Autofac.Test.Scenarios.WithProperty
{
    public class WithProps
    {
        public string A { get; set; }

        public bool B { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "StyleCop.CSharp.MaintainabilityRules",
            "SA1401:Fields should be private",
            Justification = "Tests")]
        public string _field;
#pragma warning restore SA1401 // Fields should be private
    }
}
