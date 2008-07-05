using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Remember.Model
{
    public abstract class Specification<T>
        where T : class
    {
        public bool IsSatisfiedBy(T candidate)
        {
            if (candidate == null)
                throw new ArgumentNullException("candidate");

            return SatisfiersFrom(new[] { candidate }).Any();
        }

        public IEnumerable<T> SatisfiersFrom(IEnumerable<T> candidates)
        {
            if (candidates == null)
                throw new ArgumentNullException("candidates");

            return SatisfiersFrom(candidates.AsQueryable());
        }

        public abstract IQueryable<T> SatisfiersFrom(IQueryable<T> candidates);
    }
}
