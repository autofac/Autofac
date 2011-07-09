using System;

namespace Autofac.Tests.Scenarios.ScannedAssembly
{

    public interface IHaveName
    {
        string Name { get; }
    }

    public class NameAttribute : Attribute, IHaveName
    {
        readonly string _name;

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

    [Name("My Name")]
    public class ScannedComponentWithName { }
}
