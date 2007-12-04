using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac
{
    public abstract class Service
    {
        public abstract string Description { get; }

        public override string ToString()
        {
            return Description;
        }

        public static bool operator==(Service lhs, Service rhs)
        {
            if (((object)lhs) == null)
                return ((object)rhs) == null;

            return lhs.Equals(rhs);
        }

        public static bool operator !=(Service lhs, Service rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException(
                "Subclasses of Autofac.Service must override Object.Equals()");
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException(
                "Subclasses of Autofac.Service must override Object.GetHashCode()");
        }
    }
}
