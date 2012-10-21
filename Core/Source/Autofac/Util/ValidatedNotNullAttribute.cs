using System;
using System.Collections.Generic;
using System.Linq;

namespace Autofac.Util
{
    /// <summary>
    /// Signal attribute for static analysis that indicates a helper method is
    /// validating arguments for <see langword="null" />.
    /// </summary>
    internal sealed class ValidatedNotNullAttribute : Attribute
    {
    }
}
