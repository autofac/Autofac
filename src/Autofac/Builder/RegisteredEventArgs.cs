using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Builder
{
    public class RegisteredEventArgs : EventArgs
    {
        public Container Container { get; set; }
    }
}
