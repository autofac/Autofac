using System;

namespace Autofac.Test.Scenarios.ScannedAssembly.MetadataAttributeScanningScenario
{
    public class NameAttribute : Attribute, IHaveName
    {
        private readonly string _name;

        public NameAttribute(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
