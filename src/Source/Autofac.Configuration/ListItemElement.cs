using System.Configuration;

namespace Autofac.Configuration
{
    /// <summary>
    /// Configuration for values in a list
    /// </summary>
    public class ListItemElement : ConfigurationElement
    {
        const string ValueAttributeName = "value";

        /// <summary>
        /// Gets the value to be set (will be converted.)
        /// </summary>
        /// <value>The value.</value>
        [ConfigurationProperty(ValueAttributeName, IsRequired = true)]
        public string Value
        {
            get
            {
                return (string)this[ValueAttributeName];
            }
        }
    }
}