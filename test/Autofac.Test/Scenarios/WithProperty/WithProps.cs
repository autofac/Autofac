namespace Autofac.Test.Scenarios.WithProperty
{
    public class WithProps
    {
        public string A { get; set; }

        public bool B { get; set; }

#pragma warning disable SA1401 // Fields should be private
        public string _field;
#pragma warning restore SA1401 // Fields should be private
    }
}
