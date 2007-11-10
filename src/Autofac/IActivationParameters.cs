using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac
{
    public interface IActivationParameters : IDictionary<string, object>
    {
        T Get<T>(string name);
    }
}
