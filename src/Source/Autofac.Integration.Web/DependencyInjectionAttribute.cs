using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Integration.Web
{
    /// <summary>
    /// Base class for dependency injection attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class DependencyInjectionAttribute : Attribute
    {
        ///// <summary>
        ///// If true, targets that subclass Page will be searched for
        ///// child controls that 
        ///// </summary>
        //public bool InjectChildControls { get; set; }
    }
}
