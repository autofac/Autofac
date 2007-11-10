using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac
{
    static class ServiceKeyGenerator
    {
        const string Prefix = "_SVC_";

        public static string GenerateKey(Type service)
        {
            Enforce.ArgumentNotNull(service, "service");
            return Prefix + service.AssemblyQualifiedName;
        }

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
