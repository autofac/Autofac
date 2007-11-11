using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac
{
    /// <summary>
    /// Helper class to generate unique string keys for registrations of
    /// services according to their type. Will be replaced with an
    /// encapsulated 'ServiceKey' to improve maintainability.
    /// </summary>
    static class ServiceKeyGenerator
    {
        const string Prefix = "_SVC_";

        /// <summary>
        /// Generate a string key from a service type.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static string GenerateKey(Type service)
        {
            Enforce.ArgumentNotNull(service, "service");
            return Prefix + service.AssemblyQualifiedName;
        }

        /// <summary>
        /// Format a key for display - showing the type name if
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string FormatForDisplay(string key)
        {
            Enforce.ArgumentNotNullOrEmpty(key, "key");
            if (key.StartsWith(Prefix))
                return key.Substring(Prefix.Length, key.IndexOf(',') - Prefix.Length);
            else
                return key;
        }

        public static bool TryGetService(string key, out Type service)
        {
            if (key.StartsWith(Prefix))
            {
                service = Type.GetType(key.Substring(Prefix.Length));
                return true;
            }

            service = null;
            return false;
        }
    }
}
