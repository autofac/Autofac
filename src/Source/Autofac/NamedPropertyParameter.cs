using Autofac.Util;

namespace Autofac
{
    /// <summary>
    /// Injects into properties according to their name.
    /// Needs to be reviewed - not sure if it makes sense to
    /// have properties and parameters related this way.
    /// </summary>
    public class NamedPropertyParameter : ConstantParameter
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Create a named parameter with the specified value.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The parameter value.</param>
        public NamedPropertyParameter(string name, object value)
            : base(value, pi => pi.Member.Name.Replace("set_", "") == name)
        {
            Name = Enforce.ArgumentNotNullOrEmpty(name, "name");
        }   
    }
}
