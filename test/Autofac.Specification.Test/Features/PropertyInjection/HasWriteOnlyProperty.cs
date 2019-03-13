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
                this._val = value;
            }
        }

        public string GetVal()
        {
            return this._val;
        }
    }
}
