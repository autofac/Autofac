// This software is part of the Autofac IoC container
// Copyright © 2014 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Autofac.Configuration.Util;

namespace Autofac.Configuration
{
    /// <summary>
    /// Configures properties on other modules using settings from the appSettings section of 
    /// the configuration file. These can then be provided to component constructors as parameters
    /// during registrations within the module.
    /// </summary>
    /// <remarks>
    /// The convention for the appSettings key is "Module.PropertyName" (e.g. Email.Pop3Host).
    /// Modules should be named with a "Module" suffix (e.g. EmailModule).
    /// </remarks>
    public class AppSettingsModule : Module
    {
        readonly IEnumerable<Module> _modules;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettingsModule"/> class.
        /// </summary>
        /// <param name="modules">The modules whose properties should be configured 
        /// using settings from the appSettings section of the configuration file.</param>
        public AppSettingsModule(IEnumerable<Module> modules)
        {
            _modules = modules;
        }

        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <param name="builder">The builder through which components can be registered.</param>
        /// <remarks>
        /// Note that the ContainerBuilder parameter is unique to this module.
        /// </remarks>
        protected override void Load(ContainerBuilder builder)
        {
            var settings = ConfigurationManager.AppSettings;
            var keys = settings.AllKeys;

            foreach (var setting in keys)
            {
                if (setting.Count(c => c == '.') != 1) continue;

                var parts = setting.Split('.');
                var moduleName = parts[0];
                var propertyName = parts[1];
                var value = settings[setting];

                var module = _modules.FirstOrDefault(m => m.GetType().Name == moduleName + "Module");
                if (module == null) continue;

                var property = module.GetType().GetProperty(propertyName);
                var convertedValue = TypeManipulation.ChangeToCompatibleType(value, property.PropertyType, property);

                property.SetValue(module, convertedValue, null);
            }

            foreach (var module in _modules)
                builder.RegisterModule(module);
        }
    }
}