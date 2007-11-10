using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Autofac
{
    /// <summary>
    /// Provides parameters to the resolve process, really only semantically valid
    /// when resolving factory instances right now.
    /// </summary>
	/// <remarks>
	/// Public to facilitate use from internal generated assemblies.
	/// </remarks>
    public class ActivationParameters : Dictionary<string,object>, IActivationParameters
    {
        /// <summary>
        /// No parameters.
        /// </summary>
        public static readonly ActivationParameters Empty = new ActivationParameters();

        #region IActivationParameters Members

        /// <summary>
        /// Get a parameter value cast to the provided type.
        /// </summary>
        /// <typeparam name="T">Type of the parameter value.</typeparam>
        /// <param name="name">Name of the parameter.</param>
        /// <returns>The parameter value.</returns>
        /// <exception cref="KeyNotFoundException"/>
        public T Get<T>(string name)
        {
            Enforce.ArgumentNotNull(name, "name");

            object value;
            if (TryGetValue(name, out value))
                return (T)value;

            throw new ArgumentOutOfRangeException("name");
        }

        #endregion
    }
}
