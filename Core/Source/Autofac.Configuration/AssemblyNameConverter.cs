using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Autofac.Configuration
{
    /// <summary>
    /// Type converter used for converting assembly name strings to assembly and back.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type is used in configuration settings where an assembly name is provided and needs to be
    /// handled on the back end as a strongly-typed, loaded assembly rather than a string.
    /// </para>
    /// </remarks>
    public class AssemblyNameConverter : ConfigurationConverterBase
    {
        /// <summary>
        /// Converts an assembly name into an assembly.
        /// </summary>
        /// <param name="context">
        /// The configuration context.
        /// </param>
        /// <param name="culture">
        /// The configuration culture.
        /// </param>
        /// <param name="value">
        /// The assembly name to parse.
        /// </param>
        /// <returns>
        /// If <paramref name="value" /> is <see langword="null" />, empty, or whitespace this conversion
        /// will return <see langword="null" />. Otherwise, the assembly specified by the <paramref name="value" />
        /// will be loaded and returned.
        /// </returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var assemblyName = value as String;
            if (String.IsNullOrWhiteSpace(assemblyName))
            {
                return null;
            }
            return Assembly.Load(assemblyName);
        }

        /// <summary>
        /// Converts an assembly into an assembly name.
        /// </summary>
        /// <param name="context">
        /// The configuration context.
        /// </param>
        /// <param name="culture">
        /// The configuration culture.
        /// </param>
        /// <param name="value">
        /// The assembly to convert.
        /// </param>
        /// <param name="destinationType">
        /// The destination type to which the assembly should be converted. (Ignored for configuration converters.)
        /// </param>
        /// <returns>
        /// If <paramref name="value" /> is <see langword="null" /> this conversion returns <see langword="null" />;
        /// otherwise the return value will be the full name of the assembly.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="value" /> is not <see langword="null" /> and is not an <see cref="System.Reflection.Assembly"/>.
        /// </exception>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            string result = null;
            if (value != null)
            {
                if (!typeof(Assembly).IsAssignableFrom(value.GetType()))
                {
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, ConfigurationSettingsReaderResources.TypeConversionUnsupported, value.GetType(), typeof(Assembly)));
                }
                result = ((Assembly)value).FullName;
            }
            return result;
        }
    }
}
