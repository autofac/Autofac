using System.Configuration;

namespace Autofac.Configuration
{
    /// <summary>
    /// Configuration for values in a list
    /// </summary>
    public class ListItemElement : ConfigurationElement
    {
        const string ValueAttributeName = "value";
        const string KeyAttributeName = "key";

        /// <summary>
        /// Gets the key to be set (will be converted.)
        /// </summary>
        [ConfigurationProperty(KeyAttributeName, IsRequired = false)]
        public string Key
        {
            get { return (string) this[KeyAttributeName]; }
        }

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