using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Extras.Attributed
{
    /// <summary>
    /// Allows for custom generation of metadata
    /// </summary>
    public interface IMetadataProvider
    {
        /// <summary>
        /// Gets metadata pairs for the passed target type
        /// </summary>
        /// <param name="targetType">Target type to get metadata for</param>
        /// <returns>Metadata dictionary to merge with existing metadata</returns>
        IDictionary<string, object> GetMetadata(Type targetType);
    }
}
