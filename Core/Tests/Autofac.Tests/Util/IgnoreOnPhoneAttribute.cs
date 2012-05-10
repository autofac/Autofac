using System;
using NUnit.Framework;

namespace Autofac.Tests.Util
{
#if WINDOWS_PHONE
    public class IgnoreOnPhoneAttribute : IgnoreAttribute
    {
        public IgnoreOnPhoneAttribute()
        {

        }

        public IgnoreOnPhoneAttribute(string reason)
            : base(reason)
        {

        }
    }
#else
    public class IgnoreOnPhoneAttribute : Attribute
    {
        public IgnoreOnPhoneAttribute()
        {

        }

        public IgnoreOnPhoneAttribute(string reason)
        {
            
        }
    }
#endif
}
