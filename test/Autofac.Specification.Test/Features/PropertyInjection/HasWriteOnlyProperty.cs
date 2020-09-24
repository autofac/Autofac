using System;

namespace Autofac.Specification.Test.Features.PropertyInjection
{
    public class HasWriteOnlyProperty
    {
        private string _val;

        public string Val
        {
            set
            {
                _val = value;
            }
        }

        public string GetVal()
        {
            return _val;
        }
    }
}
