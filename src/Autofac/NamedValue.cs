using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac
{
    public class NamedValue
    {
        public string Name { get; private set; }
        public object Value { get; private set; }

        public NamedValue(string name, object value)
        {
            Enforce.ArgumentNotNull(name, "name");
            Name = name;
            Value = value;
        }
    }
}
